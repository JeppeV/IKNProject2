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

		LinkLayerReceiver receiver = new LinkLayerReceiver ();
		byte[] buffer = new byte[10];
		buffer [0] = 240;
		receiver.link.send (buffer, 5);
		Console.Write ("Buffer sent");

	}

}


