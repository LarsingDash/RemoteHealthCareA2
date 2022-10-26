using ClientApplication.ServerConnection.Communication.CommandHandlers;
using DoctorApplication.MVVM.ViewModel;
using Newtonsoft.Json.Linq;

namespace DoctorApplication.Communication.CommandHandlers;

public class UpdateValues : ICommandHandler
{
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
                    session.AddDataSpeed(ob["data"]!.ToObject<JObject>()!);
                    session.AddDataHeartRate(ob["data"]!.ToObject<JObject>()!);
                }
            }
        }
        
    }
}