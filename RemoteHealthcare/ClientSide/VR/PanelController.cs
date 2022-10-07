using System.Globalization;
using ClientSide.Bike;
using Shared;

namespace ClientSide.VR;

//First the uuid of the head will be found, after that the panel will be added as a child of that head.
//Meanwhile a separate thread is waiting for this process to finish, and once it is it will update the panel at 10fps.
public class PanelController
{
    private readonly Tunnel tunnel;
    private string hudPanel = null;

    private double previousSpeed = 0;
    private double previousTime = 0;
    private double previousDist = 0;
    private double previousHeart = 0;
    
    private List<string> chatLines = new List<string>();

    public PanelController(VRClient vrClient, Tunnel tunnel)
    {
        this.tunnel = tunnel;

        vrClient.IDWaitList.Add("hudPanel", NodeID =>
        {
            hudPanel = NodeID;
        });
        
        //Finding the camera
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Find", new Dictionary<string, string>
            {
                {"_name_", "Head"}
            })}
        });

        //Sending the message to create hudPanel once the camera has been found
        vrClient.IDSearchList.Add("Head", HeadID =>
        {
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\AddPanel", new Dictionary<string, string>
                {
                    {"_name_", "hudPanel"},
                    {"_parent_", HeadID}
                })}
            });
        });
    }

    public void RunController()
    {
        //Preparing methods for when the updateThread starts running
        void HUDInfoAction()
        {
            //Get and convert the data from the bike
            var currentData = Program.GetBikeData();
            var speedRaw = currentData[DataType.Speed].ToString(CultureInfo.InvariantCulture);
            var speed = speedRaw.Substring(0, speedRaw.IndexOf('.') + 2);
            var timeRaw = currentData[DataType.ElapsedTime].ToString(CultureInfo.InvariantCulture);
            var time = timeRaw.Substring(0, timeRaw.IndexOf('.') + 2);
            var distRaw = currentData[DataType.Distance].ToString(CultureInfo.InvariantCulture);
            var dist = distRaw.Substring(0, distRaw.IndexOf('.') + 2);
            var heartRaw = currentData[DataType.HeartRate].ToString(CultureInfo.InvariantCulture);
            var heart = heartRaw.Substring(0, heartRaw.IndexOf('.') + 2);
            
            //Convert to doubles for check
            var currentSpeed = double.Parse(speed);
            var currentTime = double.Parse(time);
            var currentDist = double.Parse(dist);
            var currentHeart = double.Parse(heart);
            
            //Check if data has changed before updating VR engine
            if (Math.Abs(currentSpeed - previousSpeed) > 0.0)   DrawPanelText(speed, 64, 70, 60);
            if (Math.Abs(currentTime - previousTime) > 0.0)     DrawPanelText(time, 64, 70, 120);
            if (Math.Abs(currentDist - previousDist) > 0.0)     DrawPanelText(dist, 64, 70, 180);
            if (Math.Abs(currentHeart - previousHeart) > 0.0)   DrawPanelText(heart, 64, 70, 240);
            
            PrintChat();

            //Send a message to draw icons on the panel
            DrawPanelImage("data/NetworkEngine/images/Icons.png", 0, 128, 64, -256);
        }
        
        //Once the hudPanel has been made, run all actions to update the panel
        int i = 0;
        while (true)
        {
            if (hudPanel != null) UpdatePanel(hudPanel, HUDInfoAction);
            i++;
            if (i % 50000 == 0) FormatChat();
            Thread.Sleep(1000);
        }
    }


    //todo: make a listener for receiving messages 
    // clear chatLines
    // grab the last 9 messages and split them if they are too big for one line
    // then add them to chatLines
     void FormatChat()
    {
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
    
    //Print the last 9 lines in chat
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
            DrawPanelText(chatMessage, 12, x, y);
        }
    }

    private void DrawPanelText(string text, double size, double x, double y)
    {
        tunnel.SendTunnelMessage(new Dictionary<string, string>
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

    private void DrawPanelImage(string imageAddress, double posX, double posY, double sizeX, double sizeY)
    {
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\PanelDrawImage",
                    new Dictionary<string, string>
                    {
                        { "_panelid_", hudPanel },
                        { "_image_", imageAddress},
                        { "\"_position_\"", $"{posX}, {posY}" }, 
                        { "\"_size_\"", $"{sizeX}, {sizeY}" }, 
                    })
            },
        });
    }

    //Clear the screen - perform the given actions - update de panel
    private void UpdatePanel(string NodeID, params Action[] actions)
    {
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\ClearPanel", new Dictionary<string, string>
            {
                {"_uuid_", NodeID}
            })}
        });

        foreach (var action in actions)
        {
            action.Invoke();
        }
            
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\SwapPanel", new Dictionary<string, string>
            {
                {"_uuid_", NodeID}
            })}
        });
    }



}