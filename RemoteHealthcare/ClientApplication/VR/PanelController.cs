using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ClientApplication;
using ClientApplication.Bike;
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
    private string chatPanelId;
    private string speedDisplayed;
    private string timeDisplayed;
    private string distanceDisplayed;

    private bool update = false;
    private FixedSizedQueue<string> messageHistory;

    public PanelController(VRClient client, Tunnel tunnel)
    {
        this.client = client;
        this.tunnel = tunnel;
        this.messageHistory = new FixedSizedQueue<string>(5);
    }

    public async void Setup()
    {
        // finds Head Id 
        headId = await client.FindObjectUuid("Head");
        Logger.LogMessage(LogImportance.DebugHighlight, $"HeadId: {headId}");
        
        // creates VR Panel
        // todo: make the fields in AddPanel overrideable?
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("AddPanel", new Dictionary<string, string>()
                {
                    {"_name_", "hudPanel"}, {"_parent_", headId},
                    { "_serial_", serial}
                }, JsonFolder.Panel.Path)
            },
        }, false, true);

        // waiting for VR response before searching for chatPanel id
        await client.AddSerialCallbackTimeout(serial, ob =>
        {
            Logger.LogMessage(LogImportance.DebugHighlight, "HUD panel is added in VR scene");
                
        }, () => { }, 1000);
        hudPanelId = await client.FindObjectUuid("hudPanel");
        Logger.LogMessage(LogImportance.Information, $"HudPanelId: {hudPanelId}");
        
        
        // creates Chat Panel
        serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("AddPanel", new Dictionary<string, string>()
                {
                    {"_name_", "chatPanel"}, {"_parent_", headId},
                    { "_serial_", serial}
                }, JsonFolder.Panel.Path)
            },
        });

        // waiting for VR response before searching for chatPanel id
        await client.AddSerialCallbackTimeout(serial, ob =>
        {
            Logger.LogMessage(LogImportance.DebugHighlight, "Chat panel is added in VR scene");

        }, () => { }, 1000);
        chatPanelId = await client.FindObjectUuid("chatPanel");
        Logger.LogMessage(LogImportance.Information, $"ChatPanelId: {chatPanelId}");

        
        BikeHandler handler = App.GetBikeHandlerInstance();
        handler.Subscribe(DataType.Speed, speedRaw =>
        {
            var speed = Math.Round(speedRaw * 3.6,1);
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
                UpdateHudPanel();
                update = !update;
            }
        });
        thread.Start();
        UpdateHudPanel();
    }

    private void UpdateHudPanel()
    { 
        ClearPanel(hudPanelId);
        DrawPanelOutlines(hudPanelId);
        DrawPanelText(speedDisplayed, 64, 140, 65, hudPanelId);
        DrawPanelText(timeDisplayed, 64, 140, 125, hudPanelId);
        DrawPanelText(distanceDisplayed, 64, 140, 195, hudPanelId);    
        
        DrawPanelImage("data/NetworkEngine/images/Icons.png", 30, 102, 64, -192);
        SwapPanel(hudPanelId);
    }

    private async void SwapPanel(string panelId)
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("SwapPanel",
                    new Dictionary<string, string>
                    {
                        { "uuid", panelId},
                        {"_serial_", serial}
                    }, JsonFolder.Panel.Path)
            }
        }, true);
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 100);

    }

    private async void DrawPanelOutlines(string panelId)
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("DrawPanelLines",
                    new Dictionary<string, string>
                    {
                        { "uuid", panelId },
                        {"_serial_", serial}
                    }, JsonFolder.Panel.Path)
            }
        }, true);
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 100);
    }

    private async void ClearPanel(string panelId)
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("ClearPanel",
                    new Dictionary<string, string>
                    {
                        {"uuid", panelId},
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
    private async void DrawPanelText(string text, double size, double x, double y, string panelId)
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("PanelDrawText",
                    new Dictionary<string, string>
                    {
                        { "uuid", panelId },
                        { "_text_", text },
                        {"\"_size_\"", $"{size}"},
                        { "\"_position_\"", $"{x}, {y}"},
                        {"_serial_", serial}
                    }, JsonFolder.Panel.Path)
            },
        }, true);
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 1000);
    }

    public void UpdateChat(string message)
    {
        messageHistory.Enqueue(message);
        
        ClearPanel(chatPanelId);
        DrawPanelOutlines(chatPanelId);
        
        int i = 0;
        foreach (var m in messageHistory)
        {
            DrawPanelText($"Dokter: {message}", 12, 10, 10 + i * 10, chatPanelId);
            i++;
        }
        SwapPanel(chatPanelId);
    }
}