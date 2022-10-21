using System;

namespace ClientApplication.Model;

public class MessageModel
{
	public String userName { get; set; }
	public String message { get; set; }
	public string time { get; set; }
	public bool isNativeOrigin { get; set; }
	public bool? firstMessage { get; set; }

	public MessageModel(string userName, string message)
	{
		this.userName = userName;
		this.message = message;
		this.time = DateTime.Now.ToString("HH:mm");
	}
}