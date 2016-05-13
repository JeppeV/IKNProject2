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
			transportLayer = new Transport (BUFSIZE);
			input = new byte[BUFSIZE];
			while (true) {
				Console.Write ("awaiting filename from client");
				transportLayer.receive (ref input);

				var fileName = System.Text.Encoding.Default.GetString(input);
				Console.Write ("received filename from client " + filename);
				int fileSize = (int)LIB.check_File_Exists (fileName);
				sendFile (fileName, fileSize);
				Array.Clear (input, 0, input.Length);
			
			}


		}
			
		private void sendFile(String fileName, int fileSize)
		{
			byte[] output = new byte[BUFSIZE];
			if (fileSize > 0) {
				output [0] = (byte)'K';
				transportLayer.send (output, 1);
			} else {
				output [0] = (byte)'E';
				transportLayer.send (output, 1);
				return;
			}
			Console.Write ("sending file to client");

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