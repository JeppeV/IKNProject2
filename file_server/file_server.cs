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

		private System.Text.Encoding encoding = System.Text.Encoding.Default;

		private file_server ()
		{
			transportLayer = new Transport (BUFSIZE);
			input = new byte[BUFSIZE];
			int size = 0;
			Console.WriteLine ("Server: Awaiting filename from client");
			while (true) {
				while (size == 0) {
					size = transportLayer.receive (ref input);
				}
				var fileName = encoding.GetString (input).Substring(0, size);
				Console.WriteLine ("Server: Received filename from client: " + fileName);
				int fileSize = (int)LIB.check_File_Exists (fileName);
				sendFile (fileName, fileSize);
				Array.Clear (input, 0, input.Length);
				size = 0;
			}
		}
			
		private void sendFile(String fileName, int fileSize)
		{
			byte[] output = new byte[BUFSIZE];
			if (fileSize > 0) {
				Console.WriteLine ("Server: File found, sending K");
				output [0] = (byte)'K';
				transportLayer.send (output, 1);
			} else {
				Console.WriteLine ("Server: file not found, sending E");
				output [0] = (byte)'E';
				transportLayer.send (output, 1);
				return;
			}
			Console.WriteLine ("Server: Sending file to client");

			using (FileStream fs = File.Open (fileName, FileMode.Open)) {
				Console.WriteLine("Server: Reading file from: " + fs.Name);
				Array.Clear (output, 0, output.Length);
				int bytesRead = fs.Read (output, 0, BUFSIZE);
				while(bytesRead > 0){
					try{
						transportLayer.send (output, bytesRead);
						Array.Clear (output, 0, output.Length);
						bytesRead = fs.Read (output, 0, BUFSIZE);
					}catch(TimeoutException e){
						Console.WriteLine("Server: Caught TimeoutException, attempting to send same again");
					}

				}
			}

			Console.WriteLine ("Server: Finished sending file to client");
		}

		public static void Main (string[] args)
		{
			new file_server();
		}
	}
}