using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace S42B_terminal
{
	public partial class frmMain : Form
	{
		static List<CommandInfo> commands = new List<CommandInfo>()
		{
			new CommandInfo(Parameter.P | Parameter.I | Parameter.D, "Get PID", 0xB0, 0xaaaa),
			new CommandInfo(Parameter.Current, "Get Current", 0xB1, 0xaaaa),
			new CommandInfo(Parameter.Microstep, "Get Microstep", 0xB2, 0xaaaa),
			new CommandInfo(Parameter.EnablePolarity, "Get EnablePolarity", 0xB3, 0xaaaa),
			new CommandInfo(Parameter.DirPolarity, "Get DirPolarity", 0xB4, 0xaaaa),

			new CommandInfo(Parameter.None, "Read Switches*", 0xB5, 0xaaaa),
			new CommandInfo(Parameter.None, "Read Flash memory*", 0xB6, 0x00, validRange: new Tuple<ushort, ushort>(0, 0x7fff)),
			new CommandInfo(Parameter.None, "Read Angle state*", 0xB7, 0xaaaa),
			new CommandInfo(Parameter.None, "Read Angle binary*", 0x37, 0xaaaa),

			new CommandInfo(Parameter.None, "Set PID scope", 0x55, 50, x=> true),

			new CommandInfo(Parameter.P, "Set P", 0xA0, 30, x=> true),
			new CommandInfo(Parameter.I, "Set I", 0xA1, 10, x=> true),
			new CommandInfo(Parameter.D, "Set D", 0xA2, 250, x=> true),

			new CommandInfo(Parameter.P, "Set FF", 0xA7, 1, x=> true, validValues: new ushort[]{ 1, 0 }),
			new CommandInfo(Parameter.I, "Set I unwinding", 0xA8, 1, x=> true, validValues: new ushort[]{ 1, 0 }),

			new CommandInfo(Parameter.Current, "Set Current", 0xA3, 1600, validRange:new Tuple<ushort, ushort>(0, 3200)),
			new CommandInfo(Parameter.Microstep, "Set Microstep", 0xA4, 16, validValues: new ushort[]{ 2,4,8,16,32} ),

			new CommandInfo(Parameter.EnablePolarity, "Set Enable Polarity", 0xA5, 0xaa, validValues: new ushort[]{0xaa, 0x55}),
			new CommandInfo(Parameter.DirPolarity, "Set Dir Polarity", 0xA6, 0x11, validValues: new ushort[]{0x11, 0x22}),


			new CommandInfo(Parameter.ClosedLoop, "Set Closed Loop Mode", 0xA9, 0x01, validValues: new ushort[]{0x01, 0x00}),


			new CommandInfo(Parameter.None, "Start Calibration", 0xAF, 0x11),


		};

		public frmMain()
		{
			InitializeComponent();

			requireDriverCon = new Control[] { btnDiscDriver, btnStartExternal, btnStopTest, btnSend, btnPidTest };
			requireMarlinCon = new Control[] { btnPidTest, btnDiscMarlin };
		}

		private void Form1_Load(object sender, EventArgs e)
		{

			cmbCommand.DataSource = commands;
			cmbCommand.DisplayMember = "Caption";

			cmbCommand_SelectionChangeCommitted(cmbCommand, null);

			try
			{
				ManagementObjectSearcher searcher =
					new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE pnpclass = 'ports' AND name LIKE '%(COM%'");

				foreach (ManagementObject queryObj in searcher.Get())
				{
					string name = queryObj["Name"].ToString();
					var m = serialRe.Match(name);
					if (m.Success)
					{
						serialPorts.Add(new Tuple<string, string>(name, m.Groups[1].Value));
						var props = queryObj.Properties.Cast<PropertyData>().ToDictionary(x => x.Name, x => x.Value);
						Debug.WriteLine(props);
					}
				}
			}
			catch (ManagementException ex)
			{
				MessageBox.Show("An error occurred while querying for WMI data: " + ex.Message);
			}

			cmbSerialDriver.DataSource = serialPorts;
			cmbSerialDriver.DisplayMember = "Item1";
			cmbSerialDriver.ValueMember = "Item2";

			{
				var selected = Properties.Settings.Default.SerialPortDriver;
				if (selected != null) {
					var matching = serialPorts.Find(x => x.Item2 == selected);
					if (matching != null)
						cmbSerialDriver.SelectedItem = matching;
				}
			}

			cmbSerialMarlin.DataSource = serialPorts.ToList();
			cmbSerialMarlin.DisplayMember = "Item1";
			cmbSerialMarlin.ValueMember = "Item2";

			{
				var selected = Properties.Settings.Default.SerialPortMarlin;
				if (selected != null)
				{
					var matching = serialPorts.Find(x => x.Item2 == selected);
					if (matching != null)
						cmbSerialMarlin.SelectedItem = matching;
				}
			}

			cmbBaudDriver.SelectedItem = Properties.Settings.Default.SerialBaudDriver.ToString();
			cmbBaudMarlin.SelectedItem = Properties.Settings.Default.SerialBaudMarlin.ToString();

			txtGcodePrep.Text = Properties.Settings.Default.GcodePrep;
			txtGcodeTest.Text = Properties.Settings.Default.GcodeTest;

			pidDiviser = Properties.Settings.Default.PidDiviser;
			cmbPidDiviser.SelectedItem = pidDiviser.ToString();

			serialPortDriver.ReadTimeout = 200;
			serialPortMarlin.ReadTimeout = 200;
			serialPortMarlin.ReceivedBytesThreshold = 1;

			serialPortDriver.DataReceived += SerialPortDriver_DataReceived;
			serialPortDriver.ErrorReceived += SerialPortDriver_ErrorReceived;

			serialPortMarlin.DataReceived += SerialPortMarlin_DataReceived;
			serialPortMarlin.ErrorReceived += SerialPortMarlin_ErrorReceived;
			serialPortMarlin.NewLine = "\n";

			pidTabContextMenu = new ContextMenu(new MenuItem[] {
				new MenuItem("Rename...", OnTabRename),
				new MenuItem("Move to start", OnTabMoveStart),
				new MenuItem("Move to end", OnTabMoveEnd),
				new MenuItem("Save to TSV...", OnTabSaveTSV),
				new MenuItem("Save to PNG...", OnTabSavePNG),
				new MenuItem("-"),
				new MenuItem("Close", OnTabClose),
			});

		}

		private void SerialPortMarlin_ErrorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
		{
			throw new NotImplementedException();
		}

		StringBuilder sbMarlinResponse = new StringBuilder();

		Regex marlinOkRe = new Regex(@"^ok (N(\d+) )?P\d+ B(\d+)");

		private void SerialPortMarlin_DataReceived(object sender, DataReceivedArgs e)
		{
			var text = Encoding.ASCII.GetString(e.Data);
			sbMarlinResponse.Append(text);

			int start = 0;
			int lastN = -1;
			for (int i = start; i < sbMarlinResponse.Length; i++)
			{
				if (sbMarlinResponse[i] == '\n')
				{
					sbMarlinResponse.Insert(i++, '\r');
					lastN = i;
					var line = sbMarlinResponse.ToString(start, i - start - 1);

					Debug.WriteLine(string.Format("marlin: {0}", line));
					var match = marlinOkRe.Match(line);
					if (match.Success)
					{
						int lineNo = -1;
						if (match.Groups[2].Success)
						{
							lineNo = int.Parse(match.Groups[2].Value);
							
							if (marlinQueueLastLineNo.HasValue && marlinQueueLastLineNo.Value == lineNo)
							{
								
								if (marlinQueueLastLineNoNext.HasValue)
								{
									// continue after setup
									marlinQueueLastLineNo = marlinQueueLastLineNoNext.Value;
									marlinQueueLastLineNoNext = null;
									startPidSampling();
								}
								else
								{
									// we sent all the things!

									stopPidSampling();
								}
							}
						}
						var bufferAvail = int.Parse(match.Groups[3].Value);
						
						if (bufferAvail > 0)
						{
							string nextLine;
							if (marlinCommandQueue.TryDequeue(out nextLine))
							{
								serialPortMarlin.WriteLine(nextLine);
							}
						}
						
					}

					start = i + 1;
				}
			}
			
			if (lastN >= 0) {
				var newLines = sbMarlinResponse.ToString(0, lastN+1);
				Invoke(new Action(() => txtMarlinResponse.AppendText(newLines)));
				sbMarlinResponse.Remove(0, lastN + 1);
			}
			
		}

		private void stopPidSampling()
		{
			sendDriverPacket(0x55, 0);

			Task.Delay(TimeSpan.FromSeconds(1))
				.ContinueWith(new Action<Task>((t) =>
				{
					Invoke(new Action(() =>
					{
						btnPidTest.Enabled = btnDiscMarlin.Enabled;
						btnStartExternal.Enabled = true;
					}));

					List<TestPoint> copy;
					lock (pointLog)
					{
						copy = pointLog.ToList();
					}
					var pars = new Dictionary<string, int>(driverParams);
					refreshChart(copy, pars);
				}));
		}

		private void startPidSampling()
		{
			// start measuring PID error
			pointLog = new List<TestPoint>();

			driverParams["SDiv"] = pidDiviser;

			// read PID params
			sendDriverPacket(0xB0, 0xaaaa);

			// start PID sampling
			sendDriverPacket(0x55, pidDiviser);
		}

		private void sendDriverPacket(byte function, ushort value)
		{
			var packet = new byte[8];
			packet[0] = 0xfe; // header
			packet[1] = 0xfe;
			packet[2] = 5; // length
			packet[3] = function;
			packet[4] = (byte)((value >> 8) & 0xff);
			packet[5] = (byte)((value >> 0) & 0xff);
			var checksum = packet.Skip(2).Take(4).Sum(x => x);

			packet[6] = (byte)(checksum & 0xff);

			packet[7] = 0x16;

			serialPortDriver.Write(packet, 0, packet.Length);
		}

		int pageSeq = 1;

		private void refreshChart(List<TestPoint> points, Dictionary<string, int> pars)
		{
			if (!points.Any())
			{
				appendLogText("failed to log any PID datapoints\r\n");
				return;
			}
			var control = new PidResultControl(points, pars);

			BeginInvoke(new Action(() =>
			{
				var tp = new TabPage(string.Format("PID {0}", pageSeq++));
				tp.Controls.Add(control);
				control.Dock = DockStyle.Fill;

				pidPages.TryAdd(tp, control);

				tabControl1.TabPages.Add(tp);
				tabControl1.SelectedTab = tp;
			}));
		}

		ConcurrentDictionary<TabPage, PidResultControl> pidPages = new ConcurrentDictionary<TabPage, PidResultControl>();



		int? marlinQueueLastLineNo = null;
		ConcurrentQueue<string> marlinCommandQueue = new ConcurrentQueue<string>();

		private void SerialPortDriver_ErrorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
		{
			//throw new NotImplementedException();
		}

		private byte[] binaryBuffer = new byte[1024*1024*10];
		private int? binaryWritePointer = null;

		private void SerialPortDriver_DataReceived(object sender, DataReceivedArgs e)
		{
			int searchStart = 0;

			while (searchStart >= 0 && searchStart < e.Data.Length)
			{
				int textStart = searchStart;
				int textEnd = e.Data.Length;

				if (binaryWritePointer.HasValue)
				{
					searchStart = writeBinaryBuffer(e.Data, searchStart);
					continue;
				}
				else
				{
					var binaryStart = Array.FindIndex(e.Data, searchStart, x => x == 0xfe);
					if (binaryStart >= 0)
					{
						textEnd = binaryStart;
					}

					if (textEnd > textStart)
					{
						var text = Encoding.ASCII.GetString(e.Data, textStart, textEnd - textStart);
						appendLogText(text);
						searchStart = textEnd;
					}

					if (binaryStart >=0) { 
						searchStart = writeBinaryBuffer(e.Data, binaryStart);
					}
				}

			}

			
		}

		StringBuilder sb = new StringBuilder(50000);

		Regex reDriverParam = new Regex(@"^(P|I|D|FF|IU) --.* =(\d+)");

		Dictionary<string, int> driverParams = new Dictionary<string, int>()
		{
			{"SDiv", 0 },
			{"P", 0 },
			{"I", 0 },
			{"D", 0 },
			{"FF", 0 },
			{"IU", 0 },
		};

		int sbDriverReadPtr = 0;

		private void appendLogText(string text)
		{
			lock (sb)
			{
				sb.Append(text);

				int start = sbDriverReadPtr;
				for (int i = start; i < sb.Length; i++)
				{
					if (sb[i] == '\n' && i > start)
					{
						sbDriverReadPtr = i;
						var line = sb.ToString(start, i - start - 1);

						//Debug.WriteLine(string.Format("driver: {0}", line));
						var match = reDriverParam.Match(line);
						if (match.Success)
						{
							driverParams[match.Groups[1].Value] = int.Parse(match.Groups[2].Value);
						}

						start = i + 1;
					}
				}

			}
		}

		private int writeBinaryBuffer(byte[] data, int dataStart)
		{
			if (!binaryWritePointer.HasValue)
				binaryWritePointer = 0;

			var binLen = data.Length - dataStart;
			Array.Copy(data, dataStart, binaryBuffer, binaryWritePointer.Value, binLen);

			var filledLen = binaryWritePointer.Value + binLen;

			binaryWritePointer = filledLen;

			int nextPacket = data.Length;

			if (filledLen > 3)
			{
				var expectedLen = binaryBuffer[2] + 3;
				if (filledLen >= expectedLen)
				{
					// complete packet

					bool valid = true;

					valid &= (binaryBuffer[1] == 0xfe); // second header byte
					valid &= (binaryBuffer[expectedLen - 1] == 0x16); // tail

					var crc = binaryBuffer.Skip(2).Take(expectedLen - 4).Sum(x => x) & 0xff;
					valid &= (binaryBuffer[expectedLen - 2] == crc);

					if (valid)
					{

						var func = binaryBuffer[3];
						if (func == 0x37)
						{
							// read coordinates

							var angle =
								binaryBuffer[4] << 24
								| binaryBuffer[5] << 16
								| binaryBuffer[6] << 8
								| binaryBuffer[7] << 0;

							appendLogText(string.Format("[binary] angle: {0:0.00} ({1})\r\n", 360.0 * angle / 0x4000, angle));
						}
						else if (func == 0x55)
						{

							// pid scope

							TestPoint tp = new TestPoint();
							tp.Sequence = (uint)(
								  binaryBuffer[4 + 0] << 24
								| binaryBuffer[5 + 0] << 16
								| binaryBuffer[6 + 0] << 8
								| binaryBuffer[7 + 0] << 0);

							tp.PosMeasured =
								  binaryBuffer[4 + 4] << 24
								| binaryBuffer[5 + 4] << 16
								| binaryBuffer[6 + 4] << 8
								| binaryBuffer[7 + 4] << 0;

							tp.PosTarget = 
								  binaryBuffer[4 + 8] << 24
								| binaryBuffer[5 + 8] << 16
								| binaryBuffer[6 + 8] << 8
								| binaryBuffer[7 + 8] << 0;

							tp.VelMeasured =
								  binaryBuffer[4 + 12] << 24
								| binaryBuffer[5 + 12] << 16
								| binaryBuffer[6 + 12] << 8
								| binaryBuffer[7 + 12] << 0;

							tp.VelTarget =
								  binaryBuffer[4 + 16] << 24
								| binaryBuffer[5 + 16] << 16
								| binaryBuffer[6 + 16] << 8
								| binaryBuffer[7 + 16] << 0;

							tp.PidI =
								  binaryBuffer[4 + 20] << 24
								| binaryBuffer[5 + 20] << 16
								| binaryBuffer[6 + 20] << 8
								| binaryBuffer[7 + 20] << 0;


							bool log = false;
							if (Math.Abs(tp.PosError) > 3 || Math.Abs(tp.PidI) > 2000)
							{
								log = true;
								silence = silenceMax;
							}
							else if (silence > 0)
							{
								log = true;
								silence--;
							} else
							{
								// log anyway
								log = true;
							}


							if (log)
							{
								lock (pointLog)
								{
									pointLog.Add(tp);
								}
							}
						}
						var extraBytes = filledLen - expectedLen;
						nextPacket = data.Length - extraBytes;
					}
					else
					{
						// try starting at next byte
						nextPacket = 1;
					}
					binaryWritePointer = null;

				}

			}


			return nextPacket;
		}

		List<TestPoint> pointLog = new List<TestPoint>();

		static readonly int silenceMax = 30;
		int silence = silenceMax;

		static Regex serialRe = new Regex(@"\((COM\d+)\)");

		static List<Tuple<string, string>> serialPorts = new List<Tuple<string, string>>();

		ReliableSerialPort serialPortDriver = new ReliableSerialPort();

		ReliableSerialPort serialPortMarlin = new ReliableSerialPort();

		private void btnConnect_Click(object sender, EventArgs e)
		{
			if (serialPortDriver.IsOpen)
				serialPortDriver.Close();

			serialPortDriver.PortName = cmbSerialDriver.SelectedValue as string;
			serialPortDriver.BaudRate = int.Parse(cmbBaudDriver.SelectedItem.ToString());

			serialPortDriver.Open();

			onConnectionUpdate(driver: true);
		}

		private void btnConnectMarlin_Click(object sender, EventArgs e)
		{

			if (serialPortMarlin.IsOpen)
				serialPortMarlin.Close();

			serialPortMarlin.PortName = cmbSerialMarlin.SelectedValue as string;
			serialPortMarlin.BaudRate = int.Parse(cmbBaudMarlin.Text);

			serialPortMarlin.Open();

			onConnectionUpdate(marlin: true);
		}


		private void btnDiscDriver_Click(object sender, EventArgs e)
		{
			serialPortDriver.Close();
			onConnectionUpdate(driver: false);
		}

		private void btnDiscMarlin_Click(object sender, EventArgs e)
		{
			serialPortMarlin.Close();
			onConnectionUpdate(marlin: false);
		}

		private readonly Control[] requireDriverCon;
		private readonly Control[] requireMarlinCon;

		private void onConnectionUpdate(bool? marlin = null, bool? driver = null)
		{
			var eitherChanged = false;

			if (driver.HasValue && driver.Value != driverConnected)
			{
				eitherChanged = true;
				driverConnected = driver.Value;
				btnConnect.Enabled = !driverConnected;
				foreach (var x in requireDriverCon.Except(requireMarlinCon))
				{
					x.Enabled = driverConnected;
				}
			}

			if (marlin.HasValue && marlin.Value != marlinConnected)
			{
				eitherChanged = true;
				marlinConnected = marlin.Value;
				btnConnectMarlin.Enabled = !marlinConnected;
				foreach (var x in requireMarlinCon.Except(requireDriverCon))
				{
					x.Enabled = marlinConnected;
				}
			}

			if (eitherChanged)
			{
				foreach (var x in requireDriverCon.Intersect(requireMarlinCon))
				{
					x.Enabled = marlinConnected && driverConnected;
				}
			}

		}

		private bool driverConnected = false;
		private bool marlinConnected = false;

		CommandInfo currentCommand;

		private void cmbCommand_SelectionChangeCommitted(object sender, EventArgs e)
		{
			var cmd = cmbCommand.SelectedValue as CommandInfo;

			cmbValue.Items.Clear();
			cmbValue.Enabled = false;
			lblLimits.Text = "";

			if (cmd.ValidValues != null)
			{
				cmbValue.Items.AddRange(cmd.ValidValues.Cast<object>().ToArray());
				cmbValue.DropDownStyle = ComboBoxStyle.DropDownList;
				cmbValue.SelectedItem = cmd.Default;
				cmbValue.Enabled = true;
			} else if (cmd.ValidRange != null)
			{
				cmbValue.DropDownStyle = ComboBoxStyle.DropDown;
				cmbValue.Items.Add(cmd.Default);
				lblLimits.Text = string.Format("{0} - {1}", cmd.ValidRange.Item1, cmd.ValidRange.Item2);
				cmbValue.Enabled = true;
			} else if (cmd.Setter)
			{
				cmbValue.Items.Add(cmd.Default);
				cmbValue.DropDownStyle = ComboBoxStyle.DropDown;
				cmbValue.Enabled = true;
			} else
			{
				cmbValue.Items.Add(cmd.Default);
			}
			cmbValue.SelectedItem = cmd.Default;
			currentCommand = cmd;
		}


		private void cmbPidDiviser_SelectionChangeCommitted(object sender, EventArgs e)
		{
			pidDiviser = Convert.ToUInt16(cmbPidDiviser.SelectedItem);
			Properties.Settings.Default.PidDiviser = pidDiviser;
			Properties.Settings.Default.Save();
		}

		private void btnSend_Click(object sender, EventArgs e)
		{
			var value = ushort.Parse(cmbValue.Text.ToString());
			sendDriverPacket(currentCommand.Function, value);
		}

		int lastLen = 0;

		private void tmrDumpLog_Tick(object sender, EventArgs e)
		{
			lock (sb)
			{
				if (lastLen > 0 && sb.Length == lastLen)
				{
					int lastNewline = sbDriverReadPtr;

					if (lastNewline < 1)
						return;

					var res = sb.ToString(0, lastNewline + 1);
					sb.Remove(0, lastNewline + 1);
					sbDriverReadPtr = 0;

					BeginInvoke(new Action(() => { textBox1.AppendText(res); }));

				}
				else
				{
					lastLen = sb.Length;
				}
			}
		}

		ushort pidDiviser = 0;

		private void btnPidTest_Click(object sender, EventArgs e)
		{

			
			marlinQueueLastLineNo = null;
			marlinCommandQueue = new ConcurrentQueue<string>();

			lineNumber = 1;

			foreach (var l in txtGcodePrep.Lines)
			{
				var cleanLine = l.Split(';')[0];
				if (string.IsNullOrWhiteSpace(cleanLine))
					continue;

				var numberedLine = string.Format("N{0} {1}", lineNumber++, cleanLine);
				var checksum = (uint) numberedLine.Aggregate((a, b) => (char)(a ^ b));
				marlinCommandQueue.Enqueue(string.Format("{0}*{1}", numberedLine, checksum));
				
			}
			{
				// wait for commands to complete
				var numberedLine = string.Format("N{0} M400", lineNumber++);
				var checksum = (uint)numberedLine.Aggregate((a, b) => (char)(a ^ b));
				marlinCommandQueue.Enqueue(string.Format("{0}*{1}", numberedLine, checksum));
			}
			// enqueue until this 
			marlinQueueLastLineNo = lineNumber - 1;

			foreach (var l in txtGcodeTest.Lines)
			{
				var cleanLine = l.Split(';')[0];
				if (string.IsNullOrWhiteSpace(cleanLine))
					continue;

				var numberedLine = string.Format("N{0} {1}", lineNumber++, cleanLine);
				var checksum = (uint)numberedLine.Aggregate((a, b) => (char)(a ^ b));
				marlinCommandQueue.Enqueue(string.Format("{0}*{1}", numberedLine, checksum));

			}
			{
				// wait for commands to complete
				var numberedLine = string.Format("N{0} M400", lineNumber++);
				var checksum = (uint)numberedLine.Aggregate((a, b) => (char)(a ^ b));
				marlinCommandQueue.Enqueue(string.Format("{0}*{1}", numberedLine, checksum));
			}
			marlinQueueLastLineNoNext = lineNumber - 1;

			// start commands by resetting line number
			serialPortMarlin.WriteLine("M110 N0");
			btnPidTest.Enabled = false;
			btnStartExternal.Enabled = false;
		}
		int lineNumber = 0;
		int? marlinQueueLastLineNoNext = null;

		private void cmbSerialDriver_SelectionChangeCommitted(object sender, EventArgs e)
		{
			var port = cmbSerialDriver.SelectedValue as string;
			Properties.Settings.Default.SerialPortDriver = port;
			Properties.Settings.Default.Save();
		}

		private void cmbSerialMarlin_SelectionChangeCommitted(object sender, EventArgs e)
		{
			var port = cmbSerialMarlin.SelectedValue as string;
			Properties.Settings.Default.SerialPortMarlin = port;
			Properties.Settings.Default.Save();
		}

		private void cmbBaudDriver_SelectionChangeCommitted(object sender, EventArgs e)
		{
			var baud = Convert.ToInt32(cmbBaudDriver.SelectedItem);
			Properties.Settings.Default.SerialBaudDriver = baud;
			Properties.Settings.Default.Save();
		}

		private void cmbBaudMarlin_SelectionChangeCommitted(object sender, EventArgs e)
		{
			var baud = Convert.ToInt32(cmbBaudMarlin.SelectedItem);
			Properties.Settings.Default.SerialBaudMarlin = baud;
			Properties.Settings.Default.Save();
		}

		private void btnStartExternal_Click(object sender, EventArgs e)
		{
			startPidSampling();
			btnStartExternal.Enabled = false;
		}

		private void btnStopTest_Click(object sender, EventArgs e)
		{
			marlinQueueLastLineNoNext = null;
			stopPidSampling();
		}

		private TabPage getPointedTab(TabControl tc, Point position)
		{
			// skip log tab
			for (int i = 1; i < tc.TabPages.Count; i++)
				if (tc.GetTabRect(i).Contains(position))
					return tc.TabPages[i];

			return null;
		}

		ContextMenu pidTabContextMenu = null;
		TabPage pidTabContextMenuTarget = null;

		void OnTabClose(object sender, EventArgs e)
		{
			tabControl1.TabPages.Remove(pidTabContextMenuTarget);
			PidResultControl temp;
			pidPages.TryRemove(pidTabContextMenuTarget, out temp);
			pidTabContextMenuTarget = null;

		}

		void OnTabMoveEnd(object sender, EventArgs e)
		{
			tabControl1.SuspendLayout();

			var active = tabControl1.SelectedTab;
			tabControl1.TabPages.Remove(pidTabContextMenuTarget);
			tabControl1.TabPages.Add(pidTabContextMenuTarget);
			if (active == pidTabContextMenuTarget)
				tabControl1.SelectedTab = pidTabContextMenuTarget;
			pidTabContextMenuTarget = null;

			tabControl1.ResumeLayout();
		}

		void OnTabMoveStart(object sender, EventArgs e)
		{
			tabControl1.SuspendLayout();

			var active = tabControl1.SelectedTab;
			tabControl1.TabPages.Remove(pidTabContextMenuTarget);
			tabControl1.TabPages.Insert(1, pidTabContextMenuTarget);
			if (active == pidTabContextMenuTarget)
				tabControl1.SelectedTab = pidTabContextMenuTarget;
			pidTabContextMenuTarget = null;

			tabControl1.ResumeLayout();
		}

		void OnTabRename(object sender, EventArgs e)
		{
			var newName = Prompt.ShowDialog(pidTabContextMenuTarget.Text, "Rename");
			if (newName != null)
				pidTabContextMenuTarget.Text = newName;
		}

		void OnTabSaveTSV(object sender, EventArgs e)
		{
			PidResultControl control = null;
			if (!pidPages.TryGetValue(pidTabContextMenuTarget, out control))
				return;

			var d = new SaveFileDialog()
			{
				Filter = "Tab Separated Values|*.tsv",
				Title = "Save as TSV",
				FileName = pidTabContextMenuTarget.Text + ".tsv",
				OverwritePrompt = true,
			};
			if (d.ShowDialog() == DialogResult.OK)
			{
				using (var f = new FileStream(d.FileName, FileMode.Create))
				using (var w = new StreamWriter(f, Encoding.ASCII))
				{
					control.WriteTSV(w);
				}
			}
		}

		void OnTabSavePNG(object sender, EventArgs e)
		{
			PidResultControl control = null;
			if (!pidPages.TryGetValue(pidTabContextMenuTarget, out control))
				return;

			var d = new SaveFileDialog()
			{
				Filter = "PNG Images|*.png",
				Title = "Save as PNG",
				FileName = pidTabContextMenuTarget.Text + ".png",
				OverwritePrompt = true,
			};
			if (d.ShowDialog() == DialogResult.OK)
			{
				using (var f = new FileStream(d.FileName, FileMode.Create))
				{
					control.Dock = DockStyle.None;
					control.Width = 1920;
					control.Height = 1080;

					var bitmap = new Bitmap(control.Width, control.Height);
					control.DrawToBitmap(bitmap, new Rectangle(0, 0, control.Width, control.Height));

					control.Dock = DockStyle.Fill;

					bitmap.Save(f, System.Drawing.Imaging.ImageFormat.Png);
				}
			}
		}

		private void tabControl1_MouseUp(object sender, MouseEventArgs e)
		{
			TabPage pointedTab = getPointedTab(tabControl1, e.Location);
			if (pointedTab != null)
			{
				if (e.Button == MouseButtons.Right)
				{
					pidTabContextMenuTarget = pointedTab;
					pidTabContextMenu.Show(this, this.PointToClient(Cursor.Position));
				}

				if (e.Button == MouseButtons.Middle)
				{
					pidTabContextMenuTarget = pointedTab;
					OnTabClose(this, e);
				}
			}
		}

		private void btnGcodePrepSave_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.GcodePrep = txtGcodePrep.Text;
			Properties.Settings.Default.Save();
		}

		private void btnGcodeTestSave_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.GcodeTest = txtGcodeTest.Text;
			Properties.Settings.Default.Save();
		}

		private void btnGcodePrepRest_Click(object sender, EventArgs e)
		{
			txtGcodePrep.Text = Properties.Settings.Default.GcodePrep;
		}

		private void btnGcodeTestRest_Click(object sender, EventArgs e)
		{
			txtGcodeTest.Text = Properties.Settings.Default.GcodeTest;
		}
	}

	[Flags]
	public enum Parameter
	{
		None = 0,
		P = 1,
		I = 2,
		D = 4,
		Current = 8,
		Microstep = 16,
		EnablePolarity = 32,
		DirPolarity = 64,
		ClosedLoop = 128
	}

	public class CommandInfo
	{
		public Parameter Parameter { get; set; }
		public bool Setter { get; set; }
		public string Name { get; set; }
		public string Caption
		{
			get
			{
				return string.Format("{0} (0x{1:X})", Name, Function);
			}
		}
		public byte Function { get; set; }
		public ushort Default { get; set; }
		public ushort? Current { get; set; }
		public Func<ushort, bool> ValidValue { get; set; }
		public Tuple<ushort, ushort> ValidRange { get; set; }
		public List<ushort> ValidValues { get; set; }

		public CommandInfo(Parameter parameter, string name, byte function, ushort @default, Func<ushort, bool> validValue = null, ICollection<ushort> validValues = null, Tuple<ushort, ushort> validRange = null)
		{
			Parameter = parameter;
			Setter = true;
			Name = name;
			Function = function;
			Default = @default;
			ValidValue = validValue;
			if (validValues != null)
				ValidValues = new List<ushort>(validValues);
			ValidRange = validRange;
		}


		public CommandInfo(Parameter parameter, string name, byte function, ushort @default)
		{
			Parameter = parameter;
			Setter = false;
			Name = name;
			Function = function;
			Default = @default;
			ValidValue = x=> x == @default;
		}
	}
}
