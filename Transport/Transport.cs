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

		private const int headerSize = 4;

		private const int maxErrorCount = 5;

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
			int size = link.receive(ref buf);

			if (size != (int)TransSize.ACKSIZE) {
				return false;
			}
			if (!checksum.checkChecksum (buf, (int)TransSize.ACKSIZE) ||
			   buf [(int)TransCHKSUM.SEQNO] != seqNo ||
			   buf [(int)TransCHKSUM.TYPE] != (int)TransType.ACK) {
				return false;
			}
				
			
			seqNo = (byte)((buf[(int)TransCHKSUM.SEQNO] + 1) % 2);
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

			byte[] sendBuffer = new byte[size + headerSize];
			sendBuffer [(int)TransCHKSUM.SEQNO] = seqNo;
			sendBuffer [(int)TransCHKSUM.TYPE] = (byte)TransType.DATA;
			Array.Copy (buf, 0, sendBuffer, headerSize, size);
			checksum.calcChecksum (ref sendBuffer, sendBuffer.Length);
			int errorCount = 0;
			link.send (sendBuffer, sendBuffer.Length);

			while (!receiveAck ()) {
				link.send (sendBuffer, sendBuffer.Length);
				if (++errorCount == maxErrorCount) {
					break;
				}

			}
			if (errorCount == maxErrorCount) {
				Console.WriteLine ("Transport: Timed out on sending item");
				throw new TimeoutException ();
			}


		}
			

		public int receive (ref byte[] buf)
		{
			byte[] receiveBuffer = new byte[BUFSIZE+(int)TransSize.ACKSIZE];
			int size = link.receive(ref receiveBuffer);
			while (!checksum.checkChecksum (receiveBuffer, size)) {
				sendAck (false, receiveBuffer);
				Array.Clear (receiveBuffer, 0, receiveBuffer.Length);
				size = link.receive (ref receiveBuffer);
			}
			// Copy data part of Transport Layer packet into receiver 'buf' array
			Array.Copy (receiveBuffer, headerSize,  buf, 0, buf.Length);
			sendAck (true, receiveBuffer);
			size -= headerSize;
			return size;
		}
			
	}
}