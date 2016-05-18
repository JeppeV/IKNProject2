using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
	class file_client
	{
		const int BUFSIZE = 1000;

		byte[] output;

		private Transport transportLayer;

		private System.Text.Encoding encoding = System.Text.Encoding.Default;


	    private file_client(String[] args)
	    {
			transportLayer = new Transport (BUFSIZE);
			// get filepath from input arguments and send to server
			string filePath = args [0];
			output = encoding.GetBytes(filePath);
			Console.WriteLine ("Client: Requesting file from server: " + encoding.GetString(output));
			transportLayer.send (output, output.Length);
			String fileName = LIB.extractFileName (filePath);
			// attempt to receive file from server
			receiveFile (fileName);
	    }



		private void receiveFile (String fileName)
		{
			byte[] input = new byte[BUFSIZE];
			// receive status message from server
			transportLayer.receive (ref input);
			Console.WriteLine ("Client: Status message from server: " + encoding.GetString(input));
			if(!(input[0] == (byte)'K')) {
				// if status message is not (O)K, abort file receipt
				Console.WriteLine ("Client: Server could not locate file, exiting application");
				return;
			}

			Console.WriteLine ("Client: Beginning receipt of file from server");
			// clear previous input and begin receipt of file from server
			Array.Clear (input, 0, input.Length);
			using (FileStream fs = new FileStream (Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, fileName), FileMode.OpenOrCreate)) {
				int size = transportLayer.receive (ref input);
				while (size > 0) {
					fs.Write (input, 0, size); 
					Array.Clear (input, 0, input.Length);
					if (size < BUFSIZE) {
						//if the buffer was not full, we must have reached the end of the transmission
						break;
					}
					size = transportLayer.receive (ref input);
				}
				fs.Flush ();
				
			}
			Console.WriteLine ("Client: File received");

		}
			

		public static void Main (string[] args)
		{
			new file_client(args);
		}
	}
}