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
    private bool vrStarted;

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
        vrStarted = true;

        // finds Head Id 
        headId = await client.FindObjectUuid("Head");
        Logger.LogMessage(LogImportance.DebugHighlight, $"HeadId: {headId}");

        // creates VR Panel
        var hudSerial = Util.RandomString();
        var hudPanelName = "hudPanel";
        hudPanelId = await AddPanel(hudPanelName, hudSerial, -1, 0, -3, 512, 512, 1);

        // creates Chat Panel
        var chatSerial = Util.RandomString();
        var chatPanelName = "chatPanel";
        chatPanelId = await AddPanel(chatPanelName, chatSerial, -0.8, -0.8, -2.9, 512, 512, 1.2);
        UpdateChat("", "");

        var handler = App.GetBikeHandlerInstance();
            // speed
        handler.Subscribe(DataType.Speed, speedRaw =>
        {
            var speed = Math.Round(speedRaw * 3.6, 1);
            speedDisplayed = speed + " km/h";
            update = true;
        });

        // subscribes to the data from BikeHandler()
            // elapsed time
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

            // distance
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
        thread.IsBackground = true;
        thread.Start();
        UpdateHudPanel();
    }

    
    
    /// <summary>
    /// Update all elements of HUD panel in VR
    /// </summary>
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

    /// <summary>
    /// Updates the chat when adding a new message
    /// Message is added to a Queue with a fixed size of 5 messages. Older messages get dequeued.
    /// If the message is longer than 32 characters, splits the message in parts
    /// and draws them seperately in VR
    /// </summary>
    /// <param name="sender">Sender of the message</param>
    /// <param name="message">Message to be displayed in VR chat panel</param>
    public void UpdateChat(string sender, string message)
    {
        if (!vrStarted) return;
        
        ClearPanel(chatPanelId);
        DrawPanelImage("data/NetworkEngine/images/ChatBox.png", 0, 100, 481, -194, chatPanelId);

        if (!String.IsNullOrEmpty(message))
        {
            message = $"{sender}: " + message;
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

    /// <summary>
    /// Adds a new panel in VR
    /// </summary>
    /// <param name="panelName">panel name used as identifier</param>
    /// <param name="z">z-position</param>
    /// <param name="x">x-position of the panel</param>
    /// <param name="y">y-position</param>
    /// <param name="serial">serial used as callback identifier</param>
    /// <param name="height">height in pixels</param>
    /// <param name="width">width in pixels</param>
    /// <param name="scale">scales the panel</param>
    private async Task<string> AddPanel(string panelName,  string serial, double x, double y, double z, int height, int width, double scale)
    {

        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("AddPanel", new Dictionary<string, string>()
                {
                    { "_name_", panelName },
                    { "_parent_", headId },
                    { "\"_position_\"", $"{x.ToString(CultureInfo.InvariantCulture)}, {y.ToString(CultureInfo.InvariantCulture)}, {z.ToString(CultureInfo.InvariantCulture)}" },
                    { "\"_scale_\"", $"{scale.ToString(CultureInfo.InvariantCulture)}"},
                    {"\"_height_\"", $"{height.ToString(CultureInfo.InvariantCulture)}"},
                    {"\"_width_\"", $"{width.ToString(CultureInfo.InvariantCulture)}"},
                    { "_serial_", serial }
                }, JsonFolder.Panel.Path)
            }
        });
        
        // waiting for VR response before searching for chatPanel id
        await client.AddSerialCallbackTimeout(serial,
            ob => { Logger.LogMessage(LogImportance.DebugHighlight, $"{panelName} is added in VR scene"); }, () => { },
            1000);
        var panelId = await client.FindObjectUuid(panelName);
        Logger.LogMessage(LogImportance.Information, $"Id of {panelName} found: {panelId}");

        return panelId;
    }


    /// <summary>
    /// Clears the panel with the given id
    /// </summary>
    /// <param name="panelId">id of the panel</param>
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
    
    /// <summary>
    /// Swaps the panel with the given id
    /// </summary>
    /// <param name="panelId">id of the panel</param>
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
    
    /// <summary>
    /// Draws a rectangle with the given panel id
    /// </summary>
    /// <param name="panelId">id of the panel</param>
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

    /// <summary>
    /// Draws an image on the panel with the given id
    /// </summary>
    /// <param name="imagePath">path of the image in the network engine</param>
    /// <param name="posX">x-position of the image</param>
    /// <param name="posY">y-position of the image</param>
    /// <param name="sizeX">width in pixels</param>
    /// <param name="sizeY">height in pixels</param>
    /// <param name="panelId">id of the panel</param>
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

    /// <summary>
    /// Draws text on the panel with the given id
    /// </summary>
    /// <param name="text">the text to be displayed on the panel</param>
    /// <param name="size">font size of the text</param>
    /// <param name="x">x-position of the text</param>
    /// <param name="y">y-position of the text</param>
    /// <param name="panelId">id of the panel</param>
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
}