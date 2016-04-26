using System;
using Linklaget;
using Transportlaget;


public class LinkLayerReceiver
{
	public Link link;

	public LinkLayerReceiver ()
	{
		link = new Link (1000 + (int)TransSize.ACKSIZE);
	}

	public static void Main (string[] args)
	{
		Console.Write ("Hello");
		LinkLayerReceiver receiver = new LinkLayerReceiver ();
		byte[] buffer = new byte[10];
		receiver.link.receive (ref buffer);
		Console.Write (buffer);

	}
}


