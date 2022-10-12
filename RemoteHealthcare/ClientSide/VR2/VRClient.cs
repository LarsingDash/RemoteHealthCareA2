using ClientSide.Helpers;
using DoctorApplication.Communication.CommandHandlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Log;

namespace ClientSide.VR2;

public class VRClient : DefaultClientConnection
{
    private Dictionary<string, ICommandHandlerVR> commandHandler = new();
    public String TunnelID;
    private Tunnel tunnel;
    public VRClient()
    {
        Init("145.48.6.10", 6666, (json, encrypted) =>
        {
            if (commandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
            {
                if (!json["id"]!.ToObject<string>()!.Equals("tunnel/send"))
                {
                    Logger.LogMessage(LogImportance.Information, $"Got message from vr-server: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
                }
                commandHandler[json["id"]!.ToObject<string>()!].HandleCommand(this, json);
            }
            else
            {
                Logger.LogMessage(LogImportance.Warn, $"Got  message from vr-server but no commandHandler found: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
            }
        }, false);
        commandHandler.Add("session/list", new SessionList());
        commandHandler.Add("tunnel/create", new CreateTunnel());
        tunnel = new Tunnel(this);
        commandHandler.Add("tunnel/send", tunnel);
        Thread.Sleep(500);
        Setup();
    }

    private void Setup()
    {
        SendData(JsonFileReader.GetObjectAsString("SessionList", new Dictionary<string, string>()
        {
        }));
    }

    public void CreateTunnel(string sessionId)
    {
        var serial = Util.RandomString();
        Logger.LogMessage(LogImportance.Information, $"Got sessionID, creating tunnel: {sessionId}");
        SendData(JsonFileReader.GetObjectAsString("CreateTunnel", new Dictionary<string, string>()
        {
            { "_id_", sessionId },
            { "_serial_", serial }
        }));
        //Code
    }
    
    public void TunnelStartup(string id)
    {
        TunnelID = id;
        RemoveObject("GroundPlane");
        RemoveObject("LeftHand");
        RemoveObject("RightHand");
        //
        // //Remove Default Objects
        // RemoveObjectRequest("GroundPlane", "RightHand", "LeftHand");
        //     
        // //Start WorldGen
        // worldGen = new WorldGen(this, tunnel);
        //
        // //Start HUDController
        // panelController = new PanelController(this, tunnel);
        // var HUDThread = new Thread(panelController.RunController);
        //
        // bikeController = new BikeController(this, tunnel);
        // var bikeAnimationThread = new Thread(bikeController.RunController);
        //
        // HUDThread.Start();
        // bikeAnimationThread.Start();
    }

    public async Task<string> FindObjectUuid(string name)
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("GetScene", new Dictionary<string, string>()
            {
                {"_serial_", serial}
            }, JsonFolder.TunnelMessages.path)}
        });
        var uuid = "";
        await AddSerialCallbackTimeout(serial, ob =>
        {
            foreach (var jToken in ob["data"]!["children"]!)
            {
                var currentObject = (JObject)jToken;
                if (currentObject.ContainsKey("name") && currentObject.ContainsKey("uuid"))
                {
                    string foundName = currentObject["name"]!.ToObject<string>()!;
                    if (name.ToLower().Equals(foundName.ToLower()))
                    {
                        uuid = currentObject["uuid"]!.ToObject<string>()!;
                        return;
                    }
                }
            }
        }, () =>
        {
            Logger.LogMessage(LogImportance.Warn, "No response from VR server when requesting scene/get" );
        }, 1000);

        return uuid;
    }

    public async void RemoveObject(string name)
    {
        string uuid = await FindObjectUuid(name);
        if (uuid.Length == 0)
        {
            Logger.LogMessage(LogImportance.Warn, $"No object found with name: {name}, (RemoveObject)");
        }
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("DeleteNodeScene", new Dictionary<string, string>()
            {
                {"_id_", uuid}
            }, JsonFolder.TunnelMessages.path)}
        });
    }
    
}