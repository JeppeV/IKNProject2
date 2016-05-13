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
			transportLayer = new Transport (BUFSIZE);
			String filePath = args [0];
			output = Encoding.ASCII.GetBytes(output);
			Console.WriteLine ("filePath: " + System.Text.Encoding.Default.GetString(output));
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
			if(input[0] == (byte) 'E') {
				return;
			}
			Console.Write ("Beginning receipt of file");
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