using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
	class file_server
	{
		/// <summary>
		/// The BUFSIZE
		/// </summary>
		private const int BUFSIZE = 1000;
		private const string APP = "FILE_SERVER";

		/// <summary>
		/// Initializes a new instance of the <see cref="file_server"/> class.
		/// </summary>
		private file_server ()
		{
			var transport = new Transport(BUFSIZE, APP);
            Console.WriteLine(" >> Server Started");

            while ((true))
            {
                try
                {
                    string serverResponse;
                    byte[] sendBytes = new byte[BUFSIZE];
                    byte[] bytesFrom = new byte[BUFSIZE];
					transport.receive(ref bytesFrom);
                    string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
					var fileLength = LIB.check_File_Exists(dataFromClient);
                    if (fileLength > 0)
                    {
                        serverResponse = "Error: File wasn't found";
                        sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                        transport.send(sendBytes, sendBytes.Length);
                        continue;
                    }
                    sendBytes = BitConverter.GetBytes(fileLength);
                    transport.send(sendBytes, sendBytes.Length);
                    sendFile(dataFromClient,fileLength,transport);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine(" >> exit");
                    Console.ReadLine();
					return;
                }
            }
		}

		/// <summary>
		/// Sends the file.
		/// </summary>
		/// <param name='fileName'>
		/// File name.
		/// </param>
		/// <param name='fileSize'>
		/// File size.
		/// </param>
		/// <param name='tl'>
		/// Tl.
		/// </param>
		private void sendFile(String fileName, long fileSize, Transport transport)
		{

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                byte[] buf = new byte[BUFSIZE];
                int offset = 0;
                while ((offset += fs.Read(buf, offset, BUFSIZE)) <= fileSize)
                {
                    transport.send(buf,BUFSIZE);
                }
            }
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main (string[] args)
		{
			new file_server();
		}
	}
}