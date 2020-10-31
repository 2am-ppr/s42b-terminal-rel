using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S42B_terminal
{
	public class ReliableSerialPort : SerialPort
	{
		public ReliableSerialPort()
		{
			DataBits = 8;
			Parity = Parity.None;
			StopBits = StopBits.One;
			Handshake = Handshake.None;
			DtrEnable = true;
			NewLine = Environment.NewLine;
			ReceivedBytesThreshold = 1024;
		}

		public ReliableSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
		{
			PortName = portName;
			BaudRate = baudRate;
			DataBits = dataBits;
			Parity = parity;
			StopBits = stopBits;
			Handshake = Handshake.None;
			DtrEnable = true;
			NewLine = Environment.NewLine;
			ReceivedBytesThreshold = 1024;
		}

		new public void Open()
		{
			base.Open();
			ContinuousRead();
		}

		new public void Close()
		{
			base.Close();
		}

		private void ContinuousRead()
		{
			byte[] buffer = new byte[1024*1024*10];
			Action kickoffRead = null;
			kickoffRead = (Action)(() => {
				if (!IsOpen)
					return;

				BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
			 {
				 try
				 {
					 if (!ar.IsCompleted || !IsOpen)
					 {
						 // port got closed halfway
						 return;
					 }
					 int count = BaseStream.EndRead(ar);
					 byte[] dst = new byte[count];
					 Buffer.BlockCopy(buffer, 0, dst, 0, count);
					 OnDataReceived(dst);
				 }
				 catch (Exception exception)
				 {
					 Console.WriteLine("OptimizedSerialPort exception !" + exception.ToString());
				 }
				 kickoffRead();
			 }, null);
			});
			kickoffRead();
		}

		public delegate void DataReceivedEventHandler(object sender, DataReceivedArgs e);
		public new event EventHandler<DataReceivedArgs> DataReceived;
		public virtual void OnDataReceived(byte[] data)
		{
			var handler = DataReceived;
			if (handler != null)
			{
				handler(this, new DataReceivedArgs { Data = data });
			}
		}
	}

	public class DataReceivedArgs : EventArgs
	{
		public byte[] Data { get; set; }
	}
}
