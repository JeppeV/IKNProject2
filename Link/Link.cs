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

			serialPort.ReadTimeout = 400;
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
			buffer [j++] = DELIMITER;
			serialPort.Write (buffer, 0, j);
			
		}

		public int receive (ref byte[] buf)
		{
			Array.Clear (buffer, 0, buffer.Length);
			int j = 0;
		
			try{
				char c = Convert.ToChar((byte)serialPort.ReadByte ());
				if (c == 'A') {
					byte current;
					int count = 0;
					while (true) {
						current = (byte) serialPort.ReadByte ();
						c = Convert.ToChar (current);
						if (c == 'A')
							break;
						else {
							buffer [count++] = current;
						}

					}
						
				
					for (int i = 0; i < count; i++) {
						current = buffer [i];
						c = Convert.ToChar (current);
						if (c == 'B') {
							i++;
							current = buffer [i];
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
			}catch(TimeoutException e){
				return 0;
			}

			return j;

		}
	}
}
