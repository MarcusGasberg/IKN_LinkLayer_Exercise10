using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
	class file_client
	{
		/// <summary>
		/// The BUFSIZE.
		/// </summary>
		private const int BUFSIZE = 1000;
		private const string APP = "FILE_CLIENT";

        private string filePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="file_client"/> class.
        /// 
        /// file_client metoden opretter en peer-to-peer forbindelse
        /// Sender en forspÃ¸rgsel for en bestemt fil om denne findes pÃ¥ serveren
        /// Modtager filen hvis denne findes eller en besked om at den ikke findes (jvf. protokol beskrivelse)
        /// Lukker alle streams og den modtagede fil
        /// Udskriver en fejl-meddelelse hvis ikke antal argumenter er rigtige
        /// </summary>
        /// <param name='args'>
        /// Filnavn med evtuelle sti.
        /// </param>
        private file_client(String[] args)
        {
            filePath = args[0];
            Console.WriteLine("Client starts...");
            var transport = new Transport(BUFSIZE,APP);
            var fileRequestBytes = Encoding.ASCII.GetBytes(filePath);
            transport.send(fileRequestBytes, fileRequestBytes.Length);
            receiveFile(filePath, transport);
        }

		/// <summary>
		/// Receives the file.
		/// </summary>
		/// <param name='fileName'>
		/// File name.
		/// </param>
		/// <param name='transport'>
		/// Transportlaget
		/// </param>
		private void receiveFile (String fileName, Transport transport)
		{

            var receiveBuf = new byte[BUFSIZE];
            if (File.Exists(filePath))
                File.Delete(filePath);
            //Receive file size
            var byteReceived = transport.receive(ref receiveBuf);
            int fileSize = BitConverter.ToInt32(receiveBuf, 0);
            Console.WriteLine($"File size is {fileSize} bytes");

            if (fileSize == 0)
            {
                Console.WriteLine("No file received");
            }
            else
            {
                int offset = 0;
                while ((offset += transport.receive(ref receiveBuf)) <= fileSize)
                {
                    WriteToFile(receiveBuf, byteReceived);
                }
                Console.WriteLine($"File received with {offset} bytes");
            }
        }

        private void WriteToFile(byte[] bytes, int bytesReceived)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Append))
                {
                    fs.Write(bytes, 0, bytesReceived);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught while writing to file: {0}", ex);
            }
        }

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name='args'>
        /// First argument: Filname
        /// </param>
        public static void Main (string[] args)
		{
            var test = "../../kaj.jpg";
            new file_client(test);
		}
	}
}