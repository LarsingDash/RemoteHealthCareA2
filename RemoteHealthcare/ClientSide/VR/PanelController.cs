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
            var currentSpeed = Double.Parse(speed);
            var currentTime = Double.Parse(time);
            var currentDist = Double.Parse(dist);
            var currentHeart = Double.Parse(heart);
            
            //Check if data has changed before updating VR engine
            if (currentSpeed != previousSpeed) HUDTextAction(speed, "50");
            if (currentTime != previousTime) HUDTextAction(time, "75");
            if (currentDist != previousDist) HUDTextAction(dist, "100");
            if (currentHeart != previousHeart) HUDTextAction(heart, "125");
            
            PrintChat();
            
            //Write all the text to the bike
            tunnel.SendTunnelMessage(new Dictionary<string, string>
            {
                {
                    "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\PanelDrawImage",
                        new Dictionary<string, string>
                        {
                            { "_panelid_", hudPanel },
                            { "_image_", "data/NetworkEngine/images/TimeIcon.png" },
                            { "\"_position_\"", "0, 0" }, 
                        })
                },
            });

            //Send a message to write the given text
            void HUDTextAction(string param, string offset)
            {
                tunnel.SendTunnelMessage(new Dictionary<string, string>
                {
                    {
                        "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\PanelDrawText",
                            new Dictionary<string, string>
                            {
                                { "_panelid_", hudPanel },
                                { "_text_", $"{param}" },
                                {"\"_size_\"", "32.0"},
                                { "\"_position_\"", $"0, {offset}" }
                            })
                    },
                });
            }
        }
        
        //Once the hudPanel has been made, run all actions to update the panel
        while (true)
        {
            if (hudPanel != null) UpdatePanel(hudPanel, HUDInfoAction);
            
            Thread.Sleep(1000);
        }
    }
    
    //Print the last 9 messages in chat
    void PrintChat()
    {
        var chatHistory = Program.getChatHistory().TakeLast(9);
        
        for (int i = 0; i < chatHistory.Count(); i++)
        {
            int y = 150 + i * 15;
            tunnel.SendTunnelMessage(new Dictionary<string, string>
            {
                {
                    "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\PanelDrawText",
                        new Dictionary<string, string>
                        {
                            { "_panelid_", hudPanel },
                            { "_text_", $"{chatHistory.ElementAt(i)}" },
                            {"\"_size_\"", "18.0"},
                            { "\"_position_\"", $"300, {y}" }
                        })
                },
            });
        }
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