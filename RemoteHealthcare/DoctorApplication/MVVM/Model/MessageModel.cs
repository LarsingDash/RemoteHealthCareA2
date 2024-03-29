using System;

namespace DoctorApplication.MVVM.Model;

public class MessageModel
{
    public String userName { get; set; }
    public String message { get; set; }
    public string time { get; set; }
    public bool isNativeOrigin { get; set; }
    public bool? firstMessage { get; set; }

    /* This is a constructor. It is a special method that is called when an object is created. */
    public MessageModel(string userName, string message)
    {
        this.userName = userName;
        this.message = message;
        this.time = DateTime.Now.ToString("HH:mm");
    }
}