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
    public String? TunnelId;
    public Tunnel? VrTunnel;
    public WorldGen? WorldGen;
    public BikeController? BikeController;
    public PanelController? PanelController;
    private bool hasStarted;

    public List<string> HideMessages = new List<string>();
    private static readonly string assetPath = "data/NetworkEngine/";
    
    //Vr settings
    public static int SelectedRoute = 6;
    public static int SelectedScenery = 1;

    private string skyboxFolder = "";
    public string TerrainD = "";
    public string TerrainN = "";
    public string Path = "";
    public string Decoration = "";
    public readonly string time = "12";
    public string Scale = "1";
    public int DecoAmount = 1000;

    /// <summary>
    /// It sets up the connection to the VR server and loads the scenery options
    /// </summary>
    /// <returns>
    /// A JSON object
    /// </returns>
    public void Setup()
    {
        if (hasStarted) return;
        hasStarted = true;

        var random = new Random();
        if (SelectedRoute == 6) SelectedRoute = random.Next(0, 5);
        if (SelectedScenery == 6) SelectedScenery = random.Next(0, 5);

        LoadSceneryOptions();
        
        HideMessages = new List<string>()
        {
            "session/list",
            "scene/node/update",
            //"route/follow/speed",
            "callback",
            "scene/panel/drawtext",
            "scene/panel/swap",
            "scene/panel/image",
            "scene/panel/clear",
            "scene/panel/drawlines",
            "scene/node/find",
            "scene/node/add"
            
        };
        Init("145.48.6.10", 6666, (json, encrypted) =>
        {
            if (commandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
            {
                if (!json["id"]!.ToObject<string>()!.Equals("tunnel/send") && !HideMessages.Contains(json["id"]!.ToObject<string>()!))
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
        VrTunnel = new Tunnel(this);
        commandHandler.Add("tunnel/send", VrTunnel);
        Thread.Sleep(500);
        SendData(JsonFileReader.GetObjectAsString("SessionList", new Dictionary<string, string>()
        {
        }, JsonFolder.Vr.Path));
    }

    /// <summary>
    /// It creates a tunnel
    /// </summary>
    /// <param name="sessionId">The session ID of the VRChat client.</param>
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
    
    /// <summary>
    /// It sets up the scene, removes the ground plane, hands, and sets the time and skybox
    /// </summary>
    /// <param name="id">The id of the tunnel.</param>
    public async Task TunnelStartup(string id)
    {
        TunnelId = id;
        var serial = Util.RandomString();

        VrTunnel.SendTunnelMessage(new Dictionary<string, string>
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

        VrTunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("SetTimeScene", new Dictionary<string, string>
            {
                {"\"_time_\"", time}
            }, JsonFolder.TunnelMessages.Path)},
        });
        
        VrTunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("SetSkybox", new Dictionary<string, string>
            {
                {"_xpos_", $"{assetPath}{skyboxFolder}/right"},
                {"_xneg_", $"{assetPath}{skyboxFolder}/left"},
                {"_ypos_", $"{assetPath}{skyboxFolder}/up"},
                {"_yneg_", $"{assetPath}{skyboxFolder}/down"},
                {"_zpos_", $"{assetPath}{skyboxFolder}/back"},
                {"_zneg_", $"{assetPath}{skyboxFolder}/front"},
            }, JsonFolder.TunnelMessages.Path)},
        });

        VrTunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("Show", new Dictionary<string, string>()
                , JsonFolder.Route.Path)},
        });
        
        //Start WorldGen
         WorldGen = new WorldGen(this, VrTunnel);
         BikeController = new BikeController(this, VrTunnel, WorldGen);
         PanelController = new PanelController(this, VrTunnel);
    }

    /// <summary>
    /// It sends a message to the VR server asking for the UUID of an object with a given name
    /// </summary>
    /// <param name="name">The name of the object you want to find.</param>
    /// <returns>
    /// The UUID of the object.
    /// </returns>
    public async Task<string> FindObjectUuid(string name)
    {
        try
        {
            var serial = Util.RandomString();
            VrTunnel.SendTunnelMessage(new Dictionary<string, string>()
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

    /// <summary>
    /// This function removes an object from the scene
    /// </summary>
    /// <param name="name">The name of the object to remove.</param>
    public async Task RemoveObject(string name)
    {
        try
        {
            string uuid = await FindObjectUuid(name);
            if (uuid.Length == 0)
            {
                Logger.LogMessage(LogImportance.Warn, $"RemoveObject: No object found with name: {name},");
            }

            VrTunnel.SendTunnelMessage(new Dictionary<string, string>()
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

    /// <summary>
    /// It loads the selected scenery options into the variables
    /// </summary>
    private void LoadSceneryOptions()
    {
        switch (SelectedScenery)
        {
            default:
                skyboxFolder = "skyboxes/yellow";
                TerrainD = "grass_autumn_orn_d";
                TerrainN = "grass_autumn_n";
                Path = "grass_ground_d";
                Decoration = "pine";
                DecoAmount = 5000;
                Scale = "4";
                break;
            
            case 1:
                skyboxFolder = "skyboxes/gray";
                TerrainD = "lava_d";
                TerrainN = "lava_n";
                Path = "lava_black_d";
                Decoration = "lava_rock";
                Scale = "1";
                DecoAmount = 750;
                break;
            
            case 2:
                skyboxFolder = "skyboxes/blue";
                TerrainD = "moss_plants_d";
                TerrainN = "moss_plants_n";
                Path = "jungle_stone_d";
                Decoration = "tropical_plant";
                Scale = "2";
                DecoAmount = 2500;
                break;
            
            case 3:
                skyboxFolder = "skyboxes/stormy";
                TerrainD = "jungle_mntn2_s";
                TerrainN = "jungle_mntn2_n";
                Path = "mntn_black_d";
                Decoration = "rock";
                DecoAmount = 2000;
                Scale = "0.01";
                break;
            
            case 4:
                skyboxFolder = "skyboxes/brown";
                TerrainD = "desert_sand_d";
                TerrainN = "desert_sand_n";
                Path = "ground_dry_d";
                Decoration = "cactus";
                DecoAmount = 200;
                break;
            
            case 5:
                skyboxFolder = "skyboxes/interstellar";
                TerrainD = "snow1_d";
                TerrainN = "snow1_d";
                Path = "snow_bumpy_d";
                Decoration = "snowman";
                Scale = "4";
                break;
        }
    }
}