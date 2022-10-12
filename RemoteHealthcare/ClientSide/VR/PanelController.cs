using System.Globalization;
using ClientSide.Bike;
using Microsoft.VisualBasic;
using Shared;

namespace ClientSide.VR;

//First the uuid of the head will be found, after that the panel will be added as a child of that head.
//Meanwhile a separate thread is waiting for this process to finish, and once it is it will update the panel at 10fps.
public class PanelController
{
    private readonly TunnelOld tunnelOld;
    private string hudPanel = null;

    private double previousSpeed = 0;
    private double previousTime = 0;
    private double previousDist = 0;
    private double previousHeart = 0;
    
    private List<string> chatLines = new List<string>();

    public PanelController(VrClient vrClient, TunnelOld tunnelOld)
    {
        this.tunnelOld = tunnelOld;

        vrClient.IDWaitList.Add("hudPanel", NodeID =>
        {
            hudPanel = NodeID;
        });
        
        //Finding the camera
        tunnelOld.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Find", new Dictionary<string, string>
            {
                {"_name_", "Head"}
            })}
        });

        //Sending the message to create hudPanel once the camera has been found
        vrClient.IDSearchList.Add("Head", HeadID =>
        {
            tunnelOld.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\AddPanel", new Dictionary<string, string>
                {
                    {"_name_", "hudPanel"},
                    {"_parent_", HeadID}
                })}
            });
        });
    }

    /// <summary>
    /// The method takes the bike data (speed, time and distance) and formats them
    /// The values are then displayed on the panel in VR using helper methods
    ///
    /// The chat text is also sent using this method (see: FormatChat() and PrintChat())
    /// </summary>
    public void RunController()
    {
        //Preparing methods for when the updateThread starts running
        void HUDInfoAction()
        {
            //Get and convert the data from the bike
            var currentData = Program.GetBikeData();
            
            //Speed
            var speedRaw = (currentData[DataType.Speed] * 3.6).ToString(CultureInfo.InvariantCulture);
            var speed = speedRaw.Substring(0, speedRaw.IndexOf('.') + 2) + " km/h";
            
            //Time
            var timeRaw = (int) currentData[DataType.ElapsedTime];
            var time = "";
            if (timeRaw / 60 < 10)
            {
                time += "0";
            }
            time += timeRaw / 60 + " : ";
            if (timeRaw % 60 < 10)
            {
                time += "0";
            }

            time += timeRaw % 60;
            
            //Distance
            var distRaw = currentData[DataType.Distance].ToString(CultureInfo.InvariantCulture);
            var distFull = distRaw.Substring(0, distRaw.IndexOf('.') + 2) + " Meters";
            
            // //Heart
            // var heartRaw = currentData[DataType.HeartRate].ToString(CultureInfo.InvariantCulture);
            // var heart = heartRaw.Substring(0, heartRaw.IndexOf('.') + 2) + " BPM";
            
            //Draw all text
            DrawPanelText(speed, 64, 140, 65);
            DrawPanelText(time, 64, 140, 125);
            DrawPanelText(distFull, 64, 140, 195);
            
            //Send a message to draw icons on the panel
            DrawPanelImage("data/NetworkEngine/images/Icons.png", 30, 102, 64, -192);
        }
        
        //Once the hudPanel has been made, run all actions to update the panel
        while (true)
        {
            if (hudPanel != null)
            {
                UpdatePanel(hudPanel, HUDInfoAction);
            }
            Thread.Sleep(500);
        }
    }

    /// <summary>
    /// Adds an outline to the VR panel
    /// </summary>
    private void DrawPanelOutlines()
    {
        tunnelOld.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\DrawPanelLines",
                    new Dictionary<string, string>
                    {
                        {"uuid", hudPanel}
                    })
            },
        });
    }
    
    
    /// <summary>
    /// Clears previously saved lines
    /// Grabs the last 9 string inputs from a test list
    /// Splits a string if it is too big
    /// Finally puts them in chatLines to be printed by PrintChat()
    /// </summary>
     void FormatChat()
    {
        if (chatLines.Count==0) return;
        chatLines.Clear();
        var chatHistory = Program.getChatHistory().TakeLast(9);
        var length = 10;
        
        foreach (var chatMessage in chatHistory)
        {
            var chatString = chatMessage;
            while (chatString.Length > length)
            {
                var line = chatString.Substring(0, length);
                chatString = chatMessage.Substring(length);
                chatLines.Add(line);
            }

            if (chatString.Length > 0)
            {
                chatLines.Add(chatString);
            }
        }
    }
    
    /// <summary>
    /// Method takes the last 9 lines of strings, reverses the order and
    /// prints them one by one to the VR panel with an offset of based on the index of the string in the list
    /// </summary>
    private void PrintChat()
    {
        var printLines = chatLines.TakeLast(9)
            .Reverse()
            .ToList();
        
        for (int i = 0; i < printLines.Count(); i++)
        {
            string chatMessage = printLines.ElementAt(i);
            double x = 512 - (32 + chatMessage.Length*3.5); 
            double y = 300 - i * 15;
            DrawPanelText(chatMessage, 2, x, y);
        }
    }

    
    /// <summary>
    /// Help method for putting text on the panel
    /// Overwrites PanelDrawText.json with new values and sends it to the VR engine
    /// Sends the following properties: text, size, x and y
    /// </summary>
    /// <param name="text">The text that is to be displayed on the panel</param>
    /// <param name="size">The size of the text displayed on the panel. Smaller values, smaller text</param>
    /// <param name="x">The x-position of the text</param>
    /// <param name="y">The y-position of the text</param>
    private void DrawPanelText(string text, double size, double x, double y)
    {
        tunnelOld.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\PanelDrawText",
                    new Dictionary<string, string>
                    {
                        { "_panelid_", hudPanel },
                        { "_text_", text },
                        {"\"_size_\"", $"{size}"},
                        { "\"_position_\"", $"{x}, {y}" }
                    })
            },
        });
    }
    
    /// <summary>
    /// Help method for putting image on the panel
    /// Overwrites PanelDrawImage.json with new values and sends it to the VR engine
    /// Sends the following properties: imagePath, posX, posY, sizeX and sizeY
    /// </summary>
    /// <param name="imagePath">The path referencing the location of the image in the Networkengine folder</param>
    /// <param name="posX">The x-position of the image</param>
    /// <param name="posY">The y-position of the image</param>
    /// <param name="sizeX">Stretches image in x-axis</param>
    /// <param name="sizeY">Stretches image in y-axis</param>
    private void DrawPanelImage(string imagePath, double posX, double posY, double sizeX, double sizeY)
    {
        tunnelOld.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\PanelDrawImage",
                    new Dictionary<string, string>
                    {
                        { "_panelid_", hudPanel },
                        { "_image_", imagePath},
                        { "\"_position_\"", $"{posX}, {posY}" }, 
                        { "\"_size_\"", $"{sizeX}, {sizeY}" }, 
                    })
            },
        });
    }

    /// <summary>
    /// This method clears the screen, invokes the method(s) provided as parameters and then swaps the panel
    /// </summary>
    /// <param name="NodeID">The node ID of the panel (can be used for different panels</param>
    /// <param name="actions">The methods given as parameter</param>
    private void UpdatePanel(string NodeID, params Action[] actions)
    {
        tunnelOld.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\ClearPanel", new Dictionary<string, string>
            {
                {"_uuid_", NodeID}
            })}
        });

        DrawPanelOutlines();

        foreach (var action in actions)
        {
            action.Invoke();
        }
            
        tunnelOld.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\SwapPanel", new Dictionary<string, string>
            {
                {"_uuid_", NodeID}
            })}
        });
    }



}