using System.ComponentModel.DataAnnotations;
using System.Globalization;
using ClientSide.Bike;
using Shared;
using DataType = ClientSide.Bike.DataType;

namespace ClientSide.VR;

public class BikeController
{
    private VRClient vrClient;
    private Tunnel tunnel;
    private string bikeId;
    
    public BikeController(VRClient vrClient, Tunnel tunnel)
    {
        this.vrClient = vrClient;
        this.tunnel = tunnel;

        bikeId = null;
   
        //TODO: move bike startup code to here
    }
      //Prepare bike and add bike to scene
        public void AnimateBike()
        {
            string routeId = "";
            
            // After adding the route, prepare the to-be-added bike for following route
            vrClient.IDWaitList.Add("bike", bikeId =>
            {
                this.bikeId = bikeId;
                // Retrieve routeId that was added
                if (vrClient.SavedIDs.ContainsKey("route"))
                {
                    routeId = vrClient.SavedIDs["route"];
                }

                // look for camera id
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Find",
                            new Dictionary<string, string>()
                            {
                                { "_name_", "Camera" }
                            })
                    }
                });

                // snap camera on bike (via parent id)
                vrClient.IDSearchList.Add("Camera", cameraId =>
                {
                    tunnel.SendTunnelMessage(new Dictionary<string, string>()
                    {
                        {
                            "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\UpdateCamera",
                                new Dictionary<string, string>()
                                {
                                    {"_guid_", cameraId}, { "_parent_", bikeId }
                                })
                        }
                    });
                });
                
                // let bike follow route
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Route\\FollowRoute",
                            new Dictionary<string, string>()
                            {
                                { "routeid", routeId }, { "nodeid", bikeId.ToString() }
                            })
                    }
                });
            });

         

            

            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {
                    "\"_data_\"",
                    JsonFileReader.GetObjectAsString("TunnelMessages\\Route\\AddBike", new Dictionary<string, string>())
                },
            });
        }
    public void RunController()
    {
        var animationSpeed = 0.0;
        var followSpeed = 0.0;
        
        //Retrieve bike data (speed)
        var bikeData = Program.GetBikeData();
        var speedRaw = bikeData[DataType.Speed].ToString(CultureInfo.InvariantCulture);
        var speed = speedRaw.Substring(0, speedRaw.IndexOf('.') + 2);


        Console.WriteLine($"Current speed: {speed} ");
        //Modify the animation speed based on bike speed
        
        
        //Modify the route follow speed based on bike speed

        //Update VR engine with new speeds
    }
}