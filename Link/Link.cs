using System;
using System.IO.Ports;


namespace Linklaget
{

	public class Link
	{

		const byte DELIMITER = (byte)'A';

		private byte[] buffer;

		SerialPort serialPort;


		public Link (int BUFSIZE)
		{
			// Create a new SerialPort object with default settings.
			serialPort = new SerialPort("/dev/ttyS1", 115200, Parity.None, 8, StopBits.One);

			if(!serialPort.IsOpen)
				serialPort.Open();

			buffer = new byte[(BUFSIZE*2)];

			serialPort.ReadTimeout = 10000;
			serialPort.DiscardInBuffer ();
			serialPort.DiscardOutBuffer ();

		}


		public void send (byte[] buf, int size)
		{
		

			Array.Clear (buffer, 0, buffer.Length);
			byte current;
			char c;
			buffer [0] = DELIMITER;
			int j = 1;
			for (int i = 0; i < size; i++) {
				current = buf [i];
				c = Convert.ToChar (current);
				if (c == 'A') {
					buffer [j++] = (byte)'B';
					buffer [j++] = (byte)'C';
				} else if (c == 'B') {
					buffer [j++] = (byte)'B';
					buffer [j++] = (byte)'D';
				} else {
					buffer [j++] = current;
				}
			}
			buffer [j] = DELIMITER;
			Console.WriteLine ("Link: Attempting to send item of length: " + (j + 1));
			serialPort.Write (buffer, 0, buffer.Length);
			
		}

		public int receive (ref byte[] buf)
		{
			Array.Clear (buffer, 0, buffer.Length);
			int j = 0;
			byte[] input = new byte[buffer.Length];
			serialPort.Read (input, 0, input.Length);
			char c = Convert.ToChar (input[0]);
			if (c == 'A') {
				byte current;
				int count = 0;
				for (int i = 1; i < input.Length; i++) {
					current = input [i];
					c = Convert.ToChar (current);
					if (c == 'A') {
						break;
					} else {
						buffer [count++] = current;
					}
				
				}

				for (int x = 0; x < count; x++) {
					current = buffer [x];
					c = Convert.ToChar (current);
					if (c == 'B') {
						x++;
						current = buffer [x];
						c = Convert.ToChar (current);
						if (c == 'C') {
							buf[j++] = (byte)'A';

						} else if (c == 'D') {
							buf[j++] = (byte)'B';
						}
					} else {
						buf [j++] = current;
					}
				}



			}
			return j;

		}
	}
}
