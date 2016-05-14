using System;
using Linklaget;

/// <summary>
/// Transport.
/// </summary>
namespace Transportlaget
{
	/// <summary>
	/// Transport.
	/// </summary>
	public class Transport
	{

		private Link link;

		private Checksum checksum;

		private byte[] inputBuffer;

		private byte seqNo;

		private byte old_seqNo;

		private int errorCount;

		private const int DEFAULT_SEQNO = 2;

		private int BUFSIZE;

		public Transport (int BUFSIZE)
		{
			this.BUFSIZE = BUFSIZE;
			link = new Link(BUFSIZE+(int)TransSize.ACKSIZE);
			checksum = new Checksum();
			inputBuffer = new byte[BUFSIZE+(int)TransSize.ACKSIZE];
			seqNo = 0;
			old_seqNo = DEFAULT_SEQNO;
			errorCount = 0;
		}

		private bool receiveAck()
		{

			byte[] buf = new byte[(int)TransSize.ACKSIZE];
			//Console.WriteLine ("Transport: Receiving ack");
			int size = link.receive(ref buf);

			if (size != (int)TransSize.ACKSIZE) {
				return false;
			}
			if (!checksum.checkChecksum (buf, (int)TransSize.ACKSIZE) ||
			   buf [(int)TransCHKSUM.SEQNO] != seqNo ||
			   buf [(int)TransCHKSUM.TYPE] != (int)TransType.ACK) {
				Console.WriteLine ("Transport: received ack is false");
				return false;
			}
				
			
			seqNo = (byte)((buf[(int)TransCHKSUM.SEQNO] + 1) % 2);
			Console.WriteLine ("Transport: received ack is true");
			return true;
		}


		private void sendAck (bool ackType, byte[] receiveBuffer)
		{	
			byte[] ackBuf = new byte[(int)TransSize.ACKSIZE];
			ackBuf [(int)TransCHKSUM.SEQNO] = (byte) (ackType ? (byte)receiveBuffer [(int)TransCHKSUM.SEQNO] : (byte)(receiveBuffer [(int)TransCHKSUM.SEQNO] + 1) % 2);
			ackBuf [(int)TransCHKSUM.TYPE] = (byte)(int)TransType.ACK;
			checksum.calcChecksum (ref ackBuf, (int)TransSize.ACKSIZE);

			link.send(ackBuf, (int)TransSize.ACKSIZE);
		}

		public void send(byte[] buf, int size)
		{

			byte[] sendBuffer = new byte[size + 4];
			sendBuffer [(int)TransCHKSUM.SEQNO] = seqNo;
			sendBuffer [(int)TransCHKSUM.TYPE] = (byte)TransType.DATA;
			Array.Copy (buf, 0, sendBuffer, 4, size);
			checksum.calcChecksum (ref sendBuffer, sendBuffer.Length);
			Console.WriteLine ("Transport: Sending item");
			Console.WriteLine ("Transport: " + System.Text.Encoding.Default.GetString(sendBuffer));
			int count = 0;
			link.send (sendBuffer, sendBuffer.Length);
			Console.WriteLine ("Transport: Attempting to send: " + System.Text.Encoding.Default.GetString(sendBuffer));

			while (!receiveAck ()) {
				link.send (sendBuffer, sendBuffer.Length);
				count++;
				if (count == 5) {
					size = 0;
					break;
				}

			}
			Console.WriteLine ("Transport: Item succesfully sent with size: " + size);

		}
			

		public int receive (ref byte[] buf)
		{
			byte[] receiveBuffer = new byte[BUFSIZE+(int)TransSize.ACKSIZE];
			Console.WriteLine ("Transport: Receiving item");
			int count = 0;
			int size = link.receive(ref receiveBuffer);
			Console.WriteLine ("Transport: Attempting to receive item");
			while (!checksum.checkChecksum (receiveBuffer, size)) {
				sendAck (false, receiveBuffer);
				Array.Clear (receiveBuffer, 0, receiveBuffer.Length);
				size = link.receive (ref receiveBuffer);
				count++;
				if (count == 5) {
					size = 0;
					break;
				}
			}

			Array.Copy (receiveBuffer, 4,  buf, 0, buf.Length);
			if(size > 0) sendAck (true, receiveBuffer);
			Console.WriteLine ("Transport: Item successfully received with size: " + size);
			Console.WriteLine ("Transport: " + System.Text.Encoding.Default.GetString(buf));
			return size;
		}
	}
}