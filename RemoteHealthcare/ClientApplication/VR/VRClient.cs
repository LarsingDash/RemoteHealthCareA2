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
    public Tunnel tunnel;
    public WorldGen worldGen;
    public BikeController BikeController;
    public PanelController PanelController;
    private bool hasStarted;

    public List<string> hideMessages = new List<string>();
    
    //Vr settings
    public string skyboxFolder = "data/custom/skyboxes/yellow";

    public void Setup()
    {
        if (hasStarted) return;
        hasStarted = true;
        
        hideMessages = new List<string>()
        {
            "session/list",
            "scene/node/update",
            "route/follow/speed",
            "callback",
            "scene/panel/drawtext",
            "scene/panel/swap",
            "scene/panel/image",
            "scene/panel/clear",
            "scene/panel/drawlines"
            
        };
        Init("145.48.6.10", 6666, (json, encrypted) =>
        {
            if (commandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
            {
                if (!json["id"]!.ToObject<string>()!.Equals("tunnel/send") && !hideMessages.Contains(json["id"]!.ToObject<string>()!))
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
            {"\"_data_\"", JsonFileReader.GetObjectAsString("ResetScene", new Dictionary<string, string>
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

        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("SetTimeScene", new Dictionary<string, string>
            {
                {"\"_time_\"", "8"}
            }, JsonFolder.TunnelMessages.Path)},
        });
        
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("SetSkybox", new Dictionary<string, string>
            {
                {"_xpos_", $"{skyboxFolder}/right"},
                {"_xneg_", $"{skyboxFolder}/left"},
                {"_ypos_", $"{skyboxFolder}/up"},
                {"_yneg_", $"{skyboxFolder}/down"},
                {"_zpos_", $"{skyboxFolder}/back"},
                {"_zneg_", $"{skyboxFolder}/front"},
            }, JsonFolder.TunnelMessages.Path)},
        });

        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("Show", new Dictionary<string, string>()
                , JsonFolder.Route.Path)},
        });
        
        //Start WorldGen
         worldGen = new WorldGen(this, tunnel);
         BikeController = new BikeController(this, tunnel, worldGen);
         PanelController = new PanelController(this, tunnel);
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