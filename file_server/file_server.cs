using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
	class file_server
	{

		private const int BUFSIZE = 1000;

		private byte[] input;

		private Transport transportLayer;

		private file_server ()
		{
			System.Text.Encoding encoding = System.Text.Encoding.UTF8;
			transportLayer = new Transport (BUFSIZE);
			input = new byte[BUFSIZE];
			while (true) {
				Console.WriteLine ("awaiting filename from client");
				transportLayer.receive (ref input);

				var fileName = encoding.GetString(input);

				fileName = "/root/Desktop/IKNProject2/file_server/test.txt";
				Console.WriteLine ("received filename from client " + fileName );
				int fileSize = (int)LIB.check_File_Exists (fileName);
				sendFile (fileName, fileSize);
				Array.Clear (input, 0, input.Length);
			
			}


		}
			
		private void sendFile(String fileName, int fileSize)
		{
			byte[] output = new byte[BUFSIZE];
			if (fileSize > 0) {
				Console.WriteLine ("file found, sending K");
				output [0] = Convert.ToByte('K');
				transportLayer.send (output, 1);
			} else {
				Console.WriteLine ("file not found, sending E");
				output [0] = Convert.ToByte('E');
				transportLayer.send (output, 1);
				return;
			}
			Console.WriteLine ("sending file to client");

			using (FileStream fs = File.Open (fileName, FileMode.Open)) {
				Array.Clear (output, 0, output.Length);
				int bytesRead = fs.Read (output, 0, BUFSIZE);
				while(bytesRead > 0){
					Console.Write ("sending bytes to client");
					transportLayer.send (output, bytesRead);
					Array.Clear (output, 0, output.Length);
					bytesRead = fs.Read (output, 0, BUFSIZE);
				}
			}
		}

		public static void Main (string[] args)
		{
			new file_server();
		}
	}
}