using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace Linklaget
{
	/// <summary>
	/// Link.
	/// </summary>
	public class Link
	{
		/// <summary>
		/// The DELIMITE for slip protocol.
		/// </summary>
		const byte DELIMITER = (byte)'A';
		/// <summary>
		/// The serial port.
		/// </summary>
		SerialPort serialPort;

		/// <summary>
		/// Initializes a new instance of the <see cref="link"/> class.
		/// </summary>
		public Link (int BUFSIZE, string APP)
		{
			// Create a new SerialPort object with default settings.
			#if DEBUG
				if(APP.Equals("FILE_SERVER"))
				{
					serialPort = new SerialPort("/dev/ttyS0",115200,Parity.None,8,StopBits.One);
				}
				else
				{
					serialPort = new SerialPort("/dev/ttyS1",115200,Parity.None,8,StopBits.One);
				}
			#else
				serialPort = new SerialPort("/dev/ttyS1",115200,Parity.None,8,StopBits.One);
			#endif
			if(!serialPort.IsOpen)
				serialPort.Open();


			// Uncomment the next line to use timeout
			//serialPort.ReadTimeout = 500;

			serialPort.DiscardInBuffer ();
			serialPort.DiscardOutBuffer ();
		}

		/// <summary>
		/// Send the specified buf and size.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public void send (byte[] buf, int size)
		{
            //Replace A's and B's in the buffer according to SLIP protocol
			List<byte> listBuffer = new List<byte>();
			listBuffer.Add(DELIMITER);
			for (int i = 0; i < size; i++)
			{
				if(buf[i].Equals(DELIMITER))
				{
					listBuffer.Add((byte)'B');
					listBuffer.Add((byte)'C');
				}
				else if(buf[i].Equals((byte)'B'))
				{
					listBuffer.Add((byte)'B');
					listBuffer.Add((byte)'D');
				}
				else
				{
					listBuffer.Add(buf[i]);
				}
			}
			listBuffer.Add(DELIMITER);

			//Write buffer to /dev/ttyS1
			var sendBuf = listBuffer.ToArray();
			serialPort.Write(sendBuf, 0, sendBuf.Length);
            
		}


		/// <summary>
		/// Receive the specified buf and size.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		public int receive (ref byte[] buf)
		{
			int bytesRead = serialPort.Read(buf, 0, buf.Length);
			List<byte> receiveBufList = new List<byte>(buf);
            for (int i = 0; i < bytesRead; i++)
			{
				int j = i + 1;
				if(receiveBufList[i] == (byte)'A')
				{
					receiveBufList.RemoveAt(i);
				}
				if(receiveBufList[i] == (byte)'B')
				{
					if(receiveBufList[j] == (byte)'C')
					{
						receiveBufList[i] = (byte)'A';
						receiveBufList.RemoveAt(j);
					}
					else if(receiveBufList[j] == (byte)'D')
					{
						receiveBufList[i] = (byte)'B';
						receiveBufList.RemoveAt(j);
					}
				}
			}
			buf = receiveBufList.ToArray();
			return buf.Length;
		}
	}
}
