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
		const int BUFSIZE = 1000;

		byte[] output;

		private Transport transportLayer;


	    private file_client(String[] args)
	    {
			System.Text.Encoding encoding = System.Text.Encoding.UTF8;
			transportLayer = new Transport (BUFSIZE);
			string filePath = args [0];
			output = encoding.GetBytes(filePath);
			Console.WriteLine ("filePath: " + encoding.GetString(output));
			transportLayer.send (output, output.Length);
			Console.WriteLine ("filename sent to server");
			String fileName = LIB.extractFileName (filePath);
			receiveFile (fileName);


	    }

		static byte[] GetBytes(string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		private void receiveFile (String fileName)
		{
			byte[] input = new byte[BUFSIZE];
			transportLayer.receive (ref input);
			Console.WriteLine ("Status message: " + Encoding.UTF8.GetString(output));
			if(input[0] == Convert.ToByte('E')) {
				return;
			}
			Console.WriteLine ("Beginning receipt of file");
			Array.Clear (input, 0, input.Length);
			using (FileStream fs = new FileStream (fileName, FileMode.OpenOrCreate)) {
				int size = transportLayer.receive (ref input);
				while (size > 0) {
					fs.Write (input, 0, size);
					size = transportLayer.receive (ref input);
				}
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
			new file_client(args);
		}
	}
}