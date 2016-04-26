using System;
using Linklaget;
using Transportlaget;

public class LinkLayerSender
{
	private Link link;

	public LinkLayerSender ()
	{
		link = new Link (1000 + (int)TransSize.ACKSIZE);
	}

	public static void Main (string[] args)
	{

		LinkLayerSender receiver = new LinkLayerSender ();
		byte[] buffer = new byte[10];
		buffer [0] = 240;
		buffer [1] = (byte)'A';
		buffer [2] = 240;
		receiver.link.send (buffer, 3);
		Console.Write ("Buffer sent");

	}

}


