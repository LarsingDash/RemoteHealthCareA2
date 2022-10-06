using System;

namespace DoctorApplication.MVVM.Model;

public class MessageModel
{
    public String userName { get; set; }
    public String message { get; set; }
    public DateTime time { get; set; }
    public bool isNativeOrigin { get; set; }
    public bool? firstMessage { get; set; }

    
}