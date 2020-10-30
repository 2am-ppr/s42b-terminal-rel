﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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


			new CommandInfo(Parameter.Current, "Set Current", 0xA3, 1600, validRange:new Tuple<ushort, ushort>(0, 3200)),
			new CommandInfo(Parameter.Microstep, "Set Microstep", 0xA4, 16, validValues: new ushort[]{ 2,4,8,16,32} ),

			new CommandInfo(Parameter.EnablePolarity, "Set Enable Polarity", 0xA5, 0xaa, validValues: new ushort[]{0xaa, 0x55}),
			new CommandInfo(Parameter.DirPolarity, "Set Dir Polarity", 0xA6, 0x11, validValues: new ushort[]{0x11, 0x22}),


		};

		public frmMain()
		{
			InitializeComponent();


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

			cmbBaudDriver.SelectedIndex = 1;

			cmbSerialMarlin.DataSource = serialPorts.ToList();
			cmbSerialMarlin.DisplayMember = "Item1";
			cmbSerialMarlin.ValueMember = "Item2";

			cmbBaudMarlin.SelectedIndex = 0;

			cmbPidDivisor.SelectedIndex = 1;


			serialPortDriver.ReadTimeout = 200;
			serialPortMarlin.ReadTimeout = 200;
			serialPortMarlin.ReceivedBytesThreshold = 1;

			serialPortDriver.DataReceived += SerialPortDriver_DataReceived;
			serialPortDriver.ErrorReceived += SerialPortDriver_ErrorReceived;

			serialPortMarlin.DataReceived += SerialPortMarlin_DataReceived;
			serialPortMarlin.ErrorReceived += SerialPortMarlin_ErrorReceived;
			serialPortMarlin.NewLine = "\n";
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

									// start measuring PID error
									sequence = 0;
									pointLog = new List<TestPoint>();
									{
										var packet = new byte[8];
										packet[0] = 0xfe; // header
										packet[1] = 0xfe;
										packet[2] = 5; // length
										packet[3] = 0x55;
										var value = pidDivisor;
										packet[4] = (byte)((value >> 8) & 0xff);
										packet[5] = (byte)((value >> 0) & 0xff);
										var checksum = packet.Skip(2).Take(4).Sum(x => x);

										packet[6] = (byte)(checksum & 0xff);

										packet[7] = 0x16;

										serialPortDriver.Write(packet, 0, packet.Length);
									}
								}
								else
								{
									// we sent all the things!
									// stop measuring PID error

									var packet = new byte[8];
									packet[0] = 0xfe; // header
									packet[1] = 0xfe;
									packet[2] = 5; // length
									packet[3] = 0x55;
									var value = 0;
									packet[4] = (byte)((value >> 8) & 0xff);
									packet[5] = (byte)((value >> 0) & 0xff);
									var checksum = packet.Skip(2).Take(4).Sum(x => x);

									packet[6] = (byte)(checksum & 0xff);

									packet[7] = 0x16;

									serialPortDriver.Write(packet, 0, packet.Length);
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

		int? marlinQueueLastLineNo = null;
		ConcurrentQueue<string> marlinCommandQueue = new ConcurrentQueue<string>();

		private void SerialPortDriver_ErrorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
		{
			throw new NotImplementedException();
		}

		private byte[] binaryBuffer = new byte[1024*1024*100];
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

		private void appendLogText(string text)
		{
			lock (sb)
			{
				sb.Append(text);
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
							tp.Sequence = sequence;

							tp.PosMeasured =
								  binaryBuffer[4 + 0] << 24
								| binaryBuffer[5 + 0] << 16
								| binaryBuffer[6 + 0] << 8
								| binaryBuffer[7 + 0] << 0;

							tp.PosTarget =
								  binaryBuffer[4 + 4] << 24
								| binaryBuffer[5 + 4] << 16
								| binaryBuffer[6 + 4] << 8
								| binaryBuffer[7 + 4] << 0;

							tp.VelMeasured = 
								  binaryBuffer[4 + 8] << 24
								| binaryBuffer[5 + 8] << 16
								| binaryBuffer[6 + 8] << 8
								| binaryBuffer[7 + 8] << 0;

							tp.VelTarget =
								  binaryBuffer[4 + 12] << 24
								| binaryBuffer[5 + 12] << 16
								| binaryBuffer[6 + 12] << 8
								| binaryBuffer[7 + 12] << 0;

							tp.PidI =
								  binaryBuffer[4 + 16] << 24
								| binaryBuffer[5 + 16] << 16
								| binaryBuffer[6 + 16] << 8
								| binaryBuffer[7 + 16] << 0;


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
								pointLog.Add(tp);
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

		int sequence = 0;

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
			serialPortDriver.DiscardInBuffer();
			btnSend.Enabled = true;
			if (serialPortMarlin.IsOpen)
				btnPidTest.Enabled = true;

			btnDiscDriver.Enabled = true;
		}

		private void btnConnectMarlin_Click(object sender, EventArgs e)
		{

			if (serialPortMarlin.IsOpen)
				serialPortMarlin.Close();

			serialPortMarlin.PortName = cmbSerialMarlin.SelectedValue as string;
			serialPortMarlin.BaudRate = int.Parse(cmbBaudMarlin.Text);

			serialPortMarlin.Open();
			serialPortMarlin.DiscardInBuffer();
			if (serialPortDriver.IsOpen)
				btnPidTest.Enabled = true;

			btnDiscMarlin.Enabled = true;
		}


		private void btnDiscDriver_Click(object sender, EventArgs e)
		{
			serialPortDriver.Close();
			btnDiscDriver.Enabled = false;
			btnConnect.Enabled = true;
		}

		private void btnDiscMarlin_Click(object sender, EventArgs e)
		{
			serialPortMarlin.Close();
			btnDiscMarlin.Enabled = false;
			btnConnectMarlin.Enabled = true;
		}

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

		private void btnSend_Click(object sender, EventArgs e)
		{
			var packet = new byte[8];
			packet[0] = 0xfe; // header
			packet[1] = 0xfe;
			packet[2] = 5; // length
			packet[3] = currentCommand.Function;
			var value = ushort.Parse(cmbValue.Text.ToString());
			packet[4] = (byte)((value >> 8) & 0xff);
			packet[5] = (byte)((value >> 0) & 0xff);
			var checksum = packet.Skip(2).Take(4).Sum(x => x);

			packet[6] = (byte)(checksum & 0xff);

			packet[7] = 0x16;

			serialPortDriver.Write(packet, 0, packet.Length);
		}

		int lastLen = 0;

		private void tmrDumpLog_Tick(object sender, EventArgs e)
		{
			lock(sb)
			{
				if (lastLen > 0 && sb.Length == lastLen)
				{
					var res = sb.ToString();
					BeginInvoke(new Action(() => { textBox1.AppendText(res); }));

					sb.Clear();
				}
				else
				{
					lastLen = sb.Length;
				}
			}
		}

		ushort pidDivisor = 0;

		private void btnPidTest_Click(object sender, EventArgs e)
		{

			
			marlinQueueLastLineNo = null;
			marlinCommandQueue = new ConcurrentQueue<string>();

			lineNumber = 1;

			pidDivisor = ushort.Parse(cmbPidDivisor.Text.ToString());

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
		}
		int lineNumber = 0;
		int? marlinQueueLastLineNoNext = null;

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
		DirPolarity = 64
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