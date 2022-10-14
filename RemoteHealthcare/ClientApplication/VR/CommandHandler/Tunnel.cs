using System.Collections.Generic;
using ClientApplication.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Log;

namespace ClientSide.VR2.CommandHandler;

public class Tunnel : ICommandHandlerVR
{
    private VRClient vrClient;
    private Dictionary<string, ICommandHandlerVR> commandHandler = new();

    private List<string> hideList = new List<string>()
    {
        "scene/node/update"
    };
    
    public Tunnel(VRClient vrClient)
    {
        this.vrClient = vrClient;
        commandHandler = new Dictionary<string, ICommandHandlerVR>()
        {
            
        };

    }

    //Helper method to send tunnelMessages without having to add the tunnelID
    public void SendTunnelMessage(Dictionary<string, string> values, bool hide = false)
    {
        values.Add("_tunnelID_", vrClient.TunnelID);
        vrClient.SendData(JsonFileReader.GetObjectAsString("SendTunnel", values, JsonFolder.Vr.Path), hide);
    }
    
    
    public void HandleCommand(VRClient client, JObject ob)
    {
        ob = ob["data"]!["data"]!.ToObject<JObject>()!;
        if (ob.ContainsKey("serial"))
        {
            var serial = ob["serial"]!.ToObject<string>();
            if (client.SerialCallbacks.ContainsKey(serial!))
            {
                Logger.LogMessage(LogImportance.Information, $"Got message from Tunnel (returning to serial): {LogColor.Gray}\n{ob.ToString(Formatting.None)}");
                client.SerialCallbacks[serial!].Invoke(ob);
                client.SerialCallbacks.Remove(serial!);
                return;
            }
            else
            {
                Logger.LogMessage(LogImportance.Warn, $"Got message from Tunnel (Serial could not be found): {LogColor.Gray}\n{ob.ToString(Formatting.None)}");
            }
        }
        if (commandHandler.ContainsKey(ob["id"]!.ToObject<string>()!))
        {
            Logger.LogMessage(LogImportance.Information, $"Got message from Tunnel: {LogColor.Gray}\n{ob.ToString(Formatting.None)}");
            commandHandler[ob["id"]!.ToObject<string>()!].HandleCommand(vrClient, ob);
        }
        else
        {
            if (!hideList.Contains(ob["id"]!.ToObject<string>()!))
            {
                Logger.LogMessage(LogImportance.Debug,
                    $"Got message from Tunnel but no commandHandler found: {LogColor.Gray}\n{ob.ToString(Formatting.None)}");
            }
        }
    }

}