using System.Globalization;
using ClientSide.Bike;
using Shared;

namespace ClientSide.VR;

//First the uuid of the head will be found, after that the panel will be added as a child of that head.
//Meanwhile a separate thread is waiting for this process to finish, and once it is it will update the panel at 10fps.
public class PanelController
{
    private VRClient vrClient;
    private readonly Tunnel tunnel;
    private string hudPanel = null;

    public PanelController(VRClient vrClient, Tunnel tunnel)
    {
        this.vrClient = vrClient;
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
            
            //Write all the text to the bike
            HUDTextAction(speed, "50");
            HUDTextAction(time, "75");
            HUDTextAction(dist, "100");
            HUDTextAction(heart, "125");

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
            
            Thread.Sleep(100);
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