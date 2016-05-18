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
			// indicate start of packet with DELIMITER 'A'
			buffer [0] = DELIMITER;
			// initialize size of packet to 1, as we added the delimiter
			int k = 1;
			// read in bytes from input buf, and perform byte stuffing
			for (int i = 0; i < size; i++) {
				current = buf [i];
				c = Convert.ToChar (current);
				if (c == 'A') {
					// if a byte equals the char 'A' insert chars 'B' and 'C' in packet instead
					buffer [k++] = (byte)'B';
					buffer [k++] = (byte)'C';
				} else if (c == 'B') {
					// if a byte equals the char 'B' insert chars 'B' and 'D' in packet instead
					buffer [k++] = (byte)'B';
					buffer [k++] = (byte)'D';
				} else {
					// else insert the byte as is
					buffer [k++] = current;
				}
			}
			// indicate end of packet with DELIMITER 'A'
			buffer [k++] = DELIMITER;

			// send buffer to SerialPort, 
			serialPort.Write (buffer, 0, k);
			
		}

		public int receive (ref byte[] buf)
		{
			// clear buffer and initialize size of input k to 0
			Array.Clear (buffer, 0, buffer.Length);
			int k = 0;
			byte current;
			// count indicates the size of the byte-stuffed link-layer packet contents
			int count = 0;
			try{
				//read first byte from SerialPort, and cast to char. If it equals 'A', it marks the beginning of a Link-Layer packet.
				char c = Convert.ToChar((byte)serialPort.ReadByte ());
				if (c == 'A') {
					while (true) {
						current = (byte) serialPort.ReadByte ();
						c = Convert.ToChar (current);
						if (c == 'A')
							// if we read a byte as the char 'A' it marks the end of a Link-Layer packet.
							break;
						else {
							// else we insert the byte as is
							buffer [count++] = current;
						}

					}

					// perform byte-destuffing to Link-Layer packet data loaded into 'buffer'
					for (int i = 0; i < count; i++) {
						current = buffer [i];
						c = Convert.ToChar (current);
						if (c == 'B') {
							i++;
							current = buffer [i];
							c = Convert.ToChar (current);
							if (c == 'C') {
								// if we see 'B' followed by 'C', insert 'A' ' 
								buf[k++] = (byte)'A';
							} else if (c == 'D') {
								// if we see 'B' followed by 'D', insert 'B' '
								buf[k++] = (byte)'B';
							}
						} else {
							// else insert byte as is
							buf [k++] = current;
						}
					}



				}
			}catch(TimeoutException){
				// if we catch a TimeoutException from the SerialPort, return size 0 to indicate failure to upper layer
				return 0;
			}
			// return the size of the byte-destuffed Link-layer packet data
			return k;

		}
	}
}
