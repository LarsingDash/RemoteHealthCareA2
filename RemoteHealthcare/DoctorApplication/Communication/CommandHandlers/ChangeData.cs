using ClientApplication.ServerConnection.Communication.CommandHandlers;
using DoctorApplication.MVVM.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace DoctorApplication.Communication.CommandHandlers;

public class UpdateValues : ICommandHandler
{
    /// <summary>
    /// It takes the data from the websocket and adds it to the session
    /// </summary>
    /// <param name="Client">The client that sent the message</param>
    /// <param name="JObject">The data received from the server.</param>
    /// <returns>
    /// The data is being returned in a JObject.
    /// </returns>
    public void HandleCommand(Client client, JObject ob)
    {
        string? uuid = ob["data"]?["uuid"]?.ToObject<string>();
        if (uuid == null)
            return;
        foreach (var user in MainViewModel.MainViewM.users)
        {
            foreach (var session in user.Sessions)
            {
                if (session.realTimeData && session.sessionUuid == uuid)
                {
                    session.AddDataDistance(ob["data"]!.ToObject<JObject>()!);
                    //JArray speedJArray = (JArray)(ob["data"]!.ToObject<JObject>()!)["heartrate"];
                    //session.TestLastSpeed = double.Parse(speedJArray.Last().ToObject<string>()!);
                    //Console.WriteLine(double.Parse(speedJArray.Last().ToObject<string>()!));
                    session.AddDataSpeed(ob["data"]!.ToObject<JObject>()!);
                    session.AddDataHeartRate(ob["data"]!.ToObject<JObject>()!);
                }
            }
        }
    }
}