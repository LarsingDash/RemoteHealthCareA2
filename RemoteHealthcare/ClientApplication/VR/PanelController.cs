using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ClientApplication;
using ClientApplication.ServerConnection.Bike;
using ClientApplication.Util;
using ClientSide.VR2.CommandHandler;
using Shared;
using Shared.Log;

namespace ClientSide.VR2;

public class PanelController
{
    private VRClient client;
    private Tunnel tunnel;

    private string headId;
    private string hudPanelId;
    private string speedDisplayed;
    private string timeDisplayed;
    private string distanceDisplayed;

    private bool update = false;


    public PanelController(VRClient client, Tunnel tunnel)
    {
        this.client = client;
        this.tunnel = tunnel;
    }

    public async void Setup()
    {
        headId = await client.FindObjectUuid("Head");
        Logger.LogMessage(LogImportance.DebugHighlight, $"HeadId: {headId}");
        
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("AddPanel", new Dictionary<string, string>()
                {
                    {"_name_", "hudPanel"}, {"_parent_", headId},
                    { "_serial_", serial}
                }, JsonFolder.Panel.Path)
            },
        });

        await client.AddSerialCallbackTimeout(serial, ob =>
        { }, () => { }, 1000);
        hudPanelId = await client.FindObjectUuid("hudPanel");
        Logger.LogMessage(LogImportance.DebugHighlight, $"HudPanelId: {hudPanelId}");
        
        BikeHandler handler = App.GetBikeHandlerInstance();
        handler.Subscribe(DataType.Speed, speedRaw =>
        {
            var speed = Math.Round(speedRaw * 3.6,1);
            // todo: format speed to 1 decimal
           speedDisplayed = speed + " km/h";
           update = true;
        });
        
        handler.Subscribe(DataType.ElapsedTime, timeRaw =>
        {
            timeDisplayed = "";
            var time = (int)timeRaw;
            if (time / 60 < 10)
            {
                timeDisplayed += "0";
            }
            timeDisplayed += time / 60 + " : ";
            if (time % 60 < 10)
            {
                timeDisplayed += "0";
            }

            timeDisplayed += time % 60;

            update = true;

        });
        
        handler.Subscribe(DataType.Distance, distRaw =>
        {
            distanceDisplayed = Math.Round(distRaw,0) + " Meters";

            update = true;
        });
        Thread thread = new Thread(start =>
        {
            while (true)
            {
                Thread.Sleep(500);
                if (!update) continue;
                UpdatePanel();
                update = !update;
            }
        });
        thread.Start();
        UpdatePanel();
    }

    private async void UpdatePanel()
    { 
        ClearPanel();
        DrawPanelOutlines();
        DrawPanelText(speedDisplayed, 64, 140, 65);
        DrawPanelText(timeDisplayed, 64, 140, 125);
        DrawPanelText(distanceDisplayed, 64, 140, 195);    
        
        DrawPanelImage("data/NetworkEngine/images/Icons.png", 30, 102, 64, -192);
        SwapPanel();
    }

    private async void SwapPanel()
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("SwapPanel",
                    new Dictionary<string, string>
                    {
                        { "uuid", hudPanelId },
                        {"_serial_", serial}
                    }, JsonFolder.Panel.Path)
            }
        }, true);
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 100);

    }

    private async void DrawPanelOutlines()
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("DrawPanelLines",
                    new Dictionary<string, string>
                    {
                        { "uuid", hudPanelId },
                        {"_serial_", serial}
                    }, JsonFolder.Panel.Path)
            }
        }, true);
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 100);
    }

    private async void ClearPanel()
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("ClearPanel",
                    new Dictionary<string, string>
                    {
                        { "uuid", hudPanelId },
                        {"_serial_", serial}
                    }, JsonFolder.Panel.Path)
            }
        }, true);
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 100);
    }

    private async void DrawPanelImage(string imagePath, double posX, double posY, double sizeX, double sizeY)
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("PanelDrawImage",
                    new Dictionary<string, string>
                    {
                        { "uuid", hudPanelId },
                        { "_image_", imagePath},
                        { "\"_position_\"", $"{posX}, {posY}" }, 
                        { "\"_size_\"", $"{sizeX}, {sizeY}" },
                        {"_serial_", serial}
                    }, JsonFolder.Panel.Path)
            },
        }, true);
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 1000);
    }
    private async void DrawPanelText(string text, double size, double x, double y)
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("PanelDrawText",
                    new Dictionary<string, string>
                    {
                        { "uuid", hudPanelId },
                        { "_text_", text },
                        {"\"_size_\"", $"{size}"},
                        { "\"_position_\"", $"{x}, {y}"},
                        {"_serial_", serial}
                    }, JsonFolder.Panel.Path)
            },
        }, true);
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 1000);
    }

}