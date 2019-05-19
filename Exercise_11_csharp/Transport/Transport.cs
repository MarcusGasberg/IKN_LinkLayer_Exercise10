using System;
using System.Linq;
using Linklaget;

/// <summary>
/// Transport.
/// </summary>
namespace Transportlaget
{
    /// <summary>
    /// Transport.
    /// </summary>
    public class Transport
    {
        /// <summary>
        /// The link.
        /// </summary>
        private Link link;
        /// <summary>
        /// The 1' complements checksum.
        /// </summary>
        private Checksum checksum;
        /// <summary>
        /// The buffer.
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// The seq no.
        /// </summary>
        private byte seqNo;
        /// <summary>
        /// The old_seq no.
        /// </summary>
        private byte old_seqNo;
        /// <summary>
        /// The error count.
        /// </summary>
        private int errorCount;
        /// <summary>
        /// The DEFAULT_SEQNO.
        /// </summary>
        private const int DEFAULT_SEQNO = 2;
        /// <summary>
        /// The data received. True = received data in receiveAck, False = not received data in receiveAck
        /// </summary>
        private bool dataReceived;
        /// <summary>
        /// The number of data the recveived.
        /// </summary>
        private int recvSize = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transport"/> class.
        /// </summary>
        public Transport (int BUFSIZE, string APP)
        {
            link = new Link(BUFSIZE+(int)TransSize.ACKSIZE, APP);
            checksum = new Checksum();
            buffer = new byte[BUFSIZE+(int)TransSize.ACKSIZE];
            seqNo = 0;
            old_seqNo = DEFAULT_SEQNO;
            errorCount = 0;
            dataReceived = false;
        }

        /// <summary>
        /// Receives the ack.
        /// </summary>
        /// <returns>
        /// The ack.
        /// </returns>
        private bool receiveAck()
        {
            recvSize = link.receive(ref buffer);
            dataReceived = true;

            if (recvSize == (int)TransSize.ACKSIZE) {
                dataReceived = false;
                if (!checksum.checkChecksum (buffer, (int)TransSize.ACKSIZE) ||
                  buffer [(int)TransCHKSUM.SEQNO] != seqNo ||
                  buffer [(int)TransCHKSUM.TYPE] != (int)TransType.ACK)
                {
                    return false;
                }
                seqNo = (byte)((buffer[(int)TransCHKSUM.SEQNO] + 1) % 2);
            }
 
            return true;
        }

        /// <summary>
        /// Sends the ack.
        /// </summary>
        /// <param name='ackType'>
        /// Ack type.
        /// </param>
        private void sendAck (bool ackType)
        {
            byte[] ackBuf = new byte[(int)TransSize.ACKSIZE];
            ackBuf [(int)TransCHKSUM.SEQNO] = (byte)
                (ackType ? (byte)buffer [(int)TransCHKSUM.SEQNO] : (byte)(buffer [(int)TransCHKSUM.SEQNO] + 1) % 2);
            ackBuf [(int)TransCHKSUM.TYPE] = (byte)(int)TransType.ACK;
            checksum.calcChecksum (ref ackBuf, (int)TransSize.ACKSIZE);
            if (++errorCount == 3) // Simulate noise
            {
                ackBuf[1]++; // Important: Only spoil a checksum-field (ackBuf[0] or ackBuf[1])
                Console.WriteLine("Noise!byte #1 is spoiled in the third transmitted ACK-package");
            }
            link.send(ackBuf, (int)TransSize.ACKSIZE);
        }

        /// <summary>
        /// Send the specified buffer and size.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>
        public void send(byte[] buf, int size)
        {
            do
            {

                byte[] sendBuf = new byte[buf.Length + 4];
                sendBuf[(int)TransCHKSUM.SEQNO] = seqNo;
                sendBuf[(int)TransCHKSUM.TYPE] = (byte)TransType.DATA;
                Array.Copy(buf, 0, sendBuf, 4, buf.Length);
                checksum.calcChecksum(ref sendBuf, sendBuf.Length);
                if (++errorCount == 3) // Simulate noise
                {
                    sendBuf[6]++; // Important: Only spoil a checksum-field (buffer[0] or buffer[1])
                    Console.WriteLine("Noise!-byte #1 is spoiled in the sixth transmission");
                }
                link.send(sendBuf, sendBuf.Length);
            } while (!receiveAck());
            old_seqNo = DEFAULT_SEQNO;
        }

        /// <summary>
        /// Receive the specified buffer.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        public int receive(ref byte[] buf)
        {
            while (true)
            {
                bool ack = receiveAck();
                if (ack)
                {
                   
                   
                    if (dataReceived)
                    {
                        if (old_seqNo == buffer[(int)TransCHKSUM.SEQNO])
                        {
                            sendAck(true);
                            continue;
                        }  //send funktion er ikke kaldt og deafult er der ikke sat til old_segNo. Vi skal derfor bare vente på næste besked sendt, hvilket vi gør med at forstsætte loop;
                        bool check=checksum.checkChecksum(buffer, buffer.Length);
                        if(!check)
                        {
                           
                            sendAck(false);
                            continue;
                        }
                        old_seqNo = buffer[(int)TransCHKSUM.SEQNO];
                        buf = buffer.Skip(4).ToArray();
                        sendAck(true);
                        return buf.Length;
                    }

                }

            }

        }
    }
}