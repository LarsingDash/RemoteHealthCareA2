using ClientSide.Helpers;
using ClientSide.VR;
using ClientSide.VR2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Log;

namespace DoctorApplication.Communication.CommandHandlers;

public class Tunnel : ICommandHandlerVR
{
    private VRClient vrClient;
    private Dictionary<string, ICommandHandlerVR> commandHandler = new();
    
    public Tunnel(VRClient vrClient)
    {
        this.vrClient = vrClient;
        commandHandler = new Dictionary<string, ICommandHandlerVR>()
        {
            
        };

    }

    //Helper method to send tunnelMessages without having to add the tunnelID
    public void SendTunnelMessage(Dictionary<string, string> values)
    {
        values.Add("_tunnelID_", vrClient.TunnelID);
        vrClient.SendData(JsonFileReader.GetObjectAsString("SendTunnel", values, JsonFolder.Json.path));
    }
    
    
    public void HandleCommand(VRClient client, JObject ob)
    {
        ob = ob["data"]!["data"]!.ToObject<JObject>()!;
        Logger.LogMessage(LogImportance.Information, $"Got message from Tunnel: {LogColor.Gray}\n{ob.ToString(Formatting.None)}");
        if (ob.ContainsKey("serial"))
        {
            var serial = ob["serial"]!.ToObject<string>();
            if (client.SerialCallbacks.ContainsKey(serial!))
            {
                client.SerialCallbacks[serial!].Invoke(ob);
                client.SerialCallbacks.Remove(serial!);
                return;
            }
        }
        if (commandHandler.ContainsKey(ob["id"]!.ToObject<string>()!))
        {
            commandHandler[ob["id"]!.ToObject<string>()!].HandleCommand(vrClient, ob);
        }
        else
        {
            Logger.LogMessage(LogImportance.Debug, $"Got message from Tunnel but no commandHandler found: {LogColor.Gray}\n{ob.ToString(Formatting.None)}");
        }
    }

}