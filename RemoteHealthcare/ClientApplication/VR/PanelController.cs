using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
        messageHistory = new FixedSizedQueue<string>(5);
    }

    /// <summary>
    /// Method finds the head id and creates 2 panels as child nodes: the HUD panel and the chat panel
    /// Also the method subscribes to the bike handler to display data in HUD panel
    /// </summary>
    public async void Setup()
    {
        // finds Head Id 
        headId = await client.FindObjectUuid("Head");
        Logger.LogMessage(LogImportance.DebugHighlight, $"HeadId: {headId}");

        // creates VR Panel
        var hudSerial = Util.RandomString();
        var hudPanelName = "hudPanel";
        AddPanel(hudPanelName, -1, 0, -3, hudSerial, 512, 512);

        // waiting for VR response before searching for chatPanel id
        await client.AddSerialCallbackTimeout(hudSerial,
            ob => { Logger.LogMessage(LogImportance.DebugHighlight, "HUD panel is added in VR scene"); }, () => { },
            1000);
        hudPanelId = await client.FindObjectUuid(hudPanelName);
        Logger.LogMessage(LogImportance.Information, $"HudPanelId: {hudPanelId}");


        // creates Chat Panel
        var chatSerial = Util.RandomString();
        var chatPanelName = "chatPanel";
        AddPanel(chatPanelName, -1, -0.65, -2.9, chatSerial, 512, 512);

        // waiting for VR response before searching for chatPanel id
        await client.AddSerialCallbackTimeout(chatSerial,
            ob => { Logger.LogMessage(LogImportance.DebugHighlight, "Chat panel is added in VR scene"); }, () => { },
            1000);
        chatPanelId = await client.FindObjectUuid(chatPanelName);
        Logger.LogMessage(LogImportance.Information, $"ChatPanelId: {chatPanelId}");

        UpdateChat("");

        var handler = App.GetBikeHandlerInstance();
        handler.Subscribe(DataType.Speed, speedRaw =>
        {
            var speed = Math.Round(speedRaw * 3.6, 1);
            speedDisplayed = speed + " km/h";
            update = true;
        });

        handler.Subscribe(DataType.ElapsedTime, timeRaw =>
        {
            timeDisplayed = "";
            var time = (int)timeRaw;
            if (time / 60 < 10) timeDisplayed += "0";
            timeDisplayed += time / 60 + " : ";
            if (time % 60 < 10) timeDisplayed += "0";

            timeDisplayed += time % 60;

            update = true;
        });

        handler.Subscribe(DataType.Distance, distRaw =>
        {
            distanceDisplayed = Math.Round(distRaw, 0) + " Meters";

            update = true;
        });
        var thread = new Thread(start =>
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
        DrawPanelText(distanceDisplayed, 60, 140, 195, hudPanelId);

        DrawPanelImage("data/NetworkEngine/images/Icons.png", 30, 102, 64, -192, hudPanelId);
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
                        { "uuid", panelId },
                        { "_serial_", serial }
                    }, JsonFolder.Panel.Path)
            }
        }, true);
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 100);
    }

    private void AddPanel(string panelName, double x, double y, double z, string serial, int height, int width)
    {
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("AddPanel", new Dictionary<string, string>()
                {
                    { "_name_", panelName },
                    { "_parent_", headId },
                    { "\"_position_\"", $"{x}, {y}, {z}" },
                    {"\"_height_\"", $"{height}"},
                    {"\"_width_\"", $"{width}"},
                    { "_serial_", serial }
                }, JsonFolder.Panel.Path)
            }
        });
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
                        { "_serial_", serial }
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
                        { "uuid", panelId },
                        { "_serial_", serial }
                    }, JsonFolder.Panel.Path)
            }
        }, true);
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 100);
    }

    private async void DrawPanelImage(string imagePath, double posX, double posY, double sizeX, double sizeY, string panelId)
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("PanelDrawImage",
                    new Dictionary<string, string>
                    {
                        { "uuid", panelId },
                        { "_image_", imagePath },
                        { "\"_position_\"", $"{posX}, {posY}" },
                        { "\"_size_\"", $"{sizeX}, {sizeY}" },
                        { "_serial_", serial }
                    }, JsonFolder.Panel.Path)
            }
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
                        { "\"_size_\"", $"{size}" },
                        { "\"_position_\"", $"{x}, {y}" },
                        { "_serial_", serial }
                    }, JsonFolder.Panel.Path)
            }
        }, true);
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 1000);
    }

    public void UpdateChat(string message)
    {
        ClearPanel(chatPanelId);
        DrawPanelImage("data/NetworkEngine/images/ChatBox.png", 0, 100, 481, -194, chatPanelId);
        
        if (!String.IsNullOrEmpty(message))
        {
            message = "Dokter: " + message;
            var output = Regex.Split(message, @"(.{1,32})(?:\s|$)|(.{32})")
                .Where(x => x.Length > 0)
                .ToList();
            output.ForEach(s => messageHistory.Enqueue(s));

            int i = 0;
            foreach (var m in messageHistory)
            {
                DrawPanelText(m, 30, 10, 60 + i * 30, chatPanelId);
                i++;
            }
            
            Logger.LogMessage(LogImportance.Debug, $"Added message to VR: {message}");
        }
        SwapPanel(chatPanelId);

    }
}