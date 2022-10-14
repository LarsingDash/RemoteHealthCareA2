using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClientApplication.Util;
using ClientSide.VR;
using ClientSide.VR2.CommandHandler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Log;
using Tunnel = ClientSide.VR2.CommandHandler.Tunnel;

namespace ClientSide.VR2;

public class VRClient : DefaultClientConnection
{
    private Dictionary<string, ICommandHandlerVR> commandHandler = new();
    public String TunnelID;
    public readonly Tunnel tunnel;
    public WorldGen worldGen;
    public BikeController BikeController;
    public PanelController PanelController;
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
        }, JsonFolder.Vr.Path));
    }

    public void CreateTunnel(string sessionId)
    {
        var serial = Util.RandomString();
        Logger.LogMessage(LogImportance.Information, $"Got sessionID, creating tunnel: {sessionId}");
        SendData(JsonFileReader.GetObjectAsString("CreateTunnel", new Dictionary<string, string>()
        {
            { "_id_", sessionId },
            { "_serial_", serial }
        }, JsonFolder.Vr.Path));
        //Code
    }
    
    public async Task TunnelStartup(string id)
    {
        TunnelID = id;
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("ResetScene", new Dictionary<string, string>()
            {
                {"_serial_", serial}
            }, JsonFolder.TunnelMessages.Path)},
        });
        await AddSerialCallbackTimeout(serial, ob =>
        {
            Logger.LogMessage(LogImportance.Information, "Scene rebuild.");
        }, () =>
        {
            
        }, 1000);
        
        await RemoveObject("GroundPlane");
        await RemoveObject("LeftHand");
        await RemoveObject("RightHand");
        
         //Start WorldGen
         worldGen = new WorldGen(this, tunnel);
         BikeController = new BikeController(this, tunnel);
         //PanelController = new PanelController(this, tunnel);
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
        try
        {
            var serial = Util.RandomString();
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {
                    "\"_data_\"", JsonFileReader.GetObjectAsString("Find", new Dictionary<string, string>()
                    {
                        {"_serial_", serial},
                        {"_name_", name}
                    }, JsonFolder.TunnelMessages.Path)
                }
            });
            var uuid = "";
            await AddSerialCallbackTimeout(serial, ob =>
                {
                    if (ob["status"]!.ToObject<string>()!.Equals("ok"))
                    {
                        uuid = ob["data"]![0]!["uuid"]!.ToObject<string>()!;
                    }
                },
                () =>
                {
                    Logger.LogMessage(LogImportance.Warn, "No response from VR server when requesting scene/get (Object: " + name + ")");
                },
                1000);
            return uuid;
        }
        catch (Exception e)
        {
            Logger.LogMessage(LogImportance.Error, "Error (Unknown Reason) ", e);
        }

        return "";
    }

    public async Task RemoveObject(string name)
    {
        try
        {
            string uuid = await FindObjectUuid(name);
            if (uuid.Length == 0)
            {
                Logger.LogMessage(LogImportance.Warn, $"RemoveObject: No object found with name: {name},");
            }

            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {
                    "\"_data_\"", JsonFileReader.GetObjectAsString("DeleteNodeScene", new Dictionary<string, string>()
                    {
                        {"_id_", uuid}
                    }, JsonFolder.TunnelMessages.Path)
                }
            });
        }
        catch (Exception e)
        {
            Logger.LogMessage(LogImportance.Error, "Error (Unknown Reason) ", e);
        }
    }
    
}