using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

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
        SerialPort serialPort2;

        byte[] buffer;
        /// <summary>
        /// Initializes a new instance of the <see cref="link"/> class.
        /// </summary>
        public Link(int BUFSIZE, string APP)
        {
            // Create a new SerialPort object with default settings.
#if DEBUG
            if (APP.Equals("FILE_SERVER"))
            {
                serialPort = new SerialPort("/dev/ttyS1", 115200, Parity.None, 8, StopBits.One);
            }
            else
            {
                serialPort = new SerialPort("/dev/ttyS1", 115200, Parity.None, 8, StopBits.One);
            }
          
#else
                serialPort = new SerialPort("/dev/ttyS1",115200,Parity.None,8,StopBits.One);
#endif
            serialPort2 = new SerialPort("/dev/ttyS2", 115200, Parity.None, 8, StopBits.One);
            if (!serialPort.IsOpen)
                serialPort.Open();
            if (!serialPort2.IsOpen)
                serialPort2.Open();

            buffer = new byte[BUFSIZE * 2];

            // Uncomment the next line to use timeout
            //serialPort.ReadTimeout = 500;

            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
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
        public void send(byte[] buf, int size)
        {
            //Replace A's and B's in the buffer according to SLIP protocol
            List<byte> listBuffer = new List<byte>();
            listBuffer.Add(DELIMITER);
            for (int i = 0; i < size; i++)
            {
                if (buf[i].Equals(DELIMITER))
                {
                    listBuffer.Add((byte)'B');
                    listBuffer.Add((byte)'C');
                }
                else if (buf[i].Equals((byte)'B'))
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
        public int receive(ref byte[] buf)
        {
            var serialBuffer = new byte[2008];
            int bytesRead;
            int oldBytesRead = 0;
           // if (buffer[0] == (byte)0)
           // {
          
                do
                {

                    bytesRead = serialPort2.Read(serialBuffer, 0, 2008);
                    Array.Copy(serialBuffer, 0, buffer, oldBytesRead, bytesRead);
                    oldBytesRead += bytesRead;


                } while (buffer[oldBytesRead-1]!=(byte)'A');
            //}

            List<byte> receiveBufList = new List<byte>();
           // receiveBufList.RemoveRange(bytesRead, receiveBufList.Count - bytesRead);
            int count = 0;
            int skipIndex = 0;
            bool intoRange = false;
            for(int j=0;j<buffer.Length;j++)
            {
                if ((buffer[j] == (byte)'A') && count != 0)
                {
                    
                    receiveBufList.AddRange(buffer.Skip(skipIndex).Take(count+1).ToArray());
                    var b = buffer.Skip(j + 1).ToArray();
                    Array.Copy(b, 0, buffer, 0, b.Length);
                    break;
                }
                if ((buffer[j] == (byte)'A' && count == 0) || intoRange)
                {
                    intoRange = true;
                    count++;
                }
                else
                {
                    skipIndex++;
                }
            }

            receiveBufList.RemoveAt(0);
            receiveBufList.RemoveAt(receiveBufList.Count-1);
            var lastbuf = new List<byte>(1000);
            for (int i = 0; i < receiveBufList.Count; i++)
            {
                int j = i + 1 <= receiveBufList.Count-1 ? i + 1 : i;
            
                if (receiveBufList[i] == (byte)'B')
                {
                    if (receiveBufList[j] == (byte)'C')
                    {

                        lastbuf.Add((byte)'A');
                        i++;
                        continue;

                    }
                    else if (receiveBufList[j] == (byte)'D')
                    {
                        lastbuf.Add((byte)'B');
                        i++;
                        continue;

                    }
                }
                lastbuf.Add(receiveBufList[i]);
            }
            buf = lastbuf.ToArray();
        
            return buf.Length;
        }
    }
}
