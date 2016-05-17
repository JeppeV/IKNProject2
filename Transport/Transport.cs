using System;
using Linklaget;

namespace Transportlaget
{
	public class Transport
	{

		private Link link;

		private Checksum checksum;

		private byte seqNo;

		private const int HEADER_SIZE = 4;

		private const int PACKET_DATA_INDEX = 4;

		private const int MAX_ERROR_COUNT = 5;

		private const int DEFAULT_SEQNO = 2;

		private int BUFSIZE;

		public Transport (int BUFSIZE)
		{
			this.BUFSIZE = BUFSIZE;
			link = new Link(BUFSIZE+(int)TransSize.ACKSIZE);
			checksum = new Checksum();
			seqNo = 0;
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
			byte[] packet = createPacket (buf, size);
			int errorCount = 0;

			link.send (packet, packet.Length);
			while (!receiveAck ()) {
				if (++errorCount == MAX_ERROR_COUNT) {
					throw new TimeoutException ();
				}
				link.send (packet, packet.Length);
			}
		}

		private byte[] createPacket(byte[] data, int size){
			byte[] packet = new byte[size + HEADER_SIZE];
			packet [(int)TransCHKSUM.SEQNO] = seqNo;
			packet [(int)TransCHKSUM.TYPE] = (byte)TransType.DATA;
			Array.Copy (data, 0, packet, PACKET_DATA_INDEX, size);
			checksum.calcChecksum (ref packet, packet.Length);
			return packet;
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
			Array.Copy (receiveBuffer, HEADER_SIZE,  buf, 0, buf.Length);
			sendAck (true, receiveBuffer);
			// Remove headerSize from size
			size -= HEADER_SIZE;
			return size;
		}
			
	}
}