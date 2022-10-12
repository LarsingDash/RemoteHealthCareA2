using System.ComponentModel.DataAnnotations;
using System.Globalization;
using ClientSide.Bike;
using Shared;
using DataType = ClientSide.Bike.DataType;

namespace ClientSide.VR;


/**
 * Manages the bike animation and the camera placement in the VR engine
 */
public class BikeController
{
    private VrClient vrClient;
    private Tunnel tunnel;
    private string? bikeId;
    private string? _routeId;
    private double previousSpeed;
    
    public BikeController(VrClient vrClient, Tunnel tunnel)
    {
        this.vrClient = vrClient;
        this.tunnel = tunnel;

        bikeId = null;
        _routeId = null;
        previousSpeed = 0;
    }
      /// <summary>
      /// This method prepares the animation of the bike and adds the bike as well the route
      /// (create action -> add bike -> add route -> action gets invoked in tunnel)
      /// Adds an Action to the IDWaitList which is invoked when the bike is added to the scene
      /// The Action retrieves the route id and bike id when found
      ///   and connects the camera node as well the route node to the bike
      /// </summary>
        public void AnimateBike()
        {
            
            // After adding the route, prepare the to-be-added bike for following route
            vrClient.IDWaitList.Add("bike", bikeId =>
            {
                this.bikeId = bikeId;
                // Retrieve routeId that was added
                if (vrClient.SavedIDs.ContainsKey("route"))
                {
                    _routeId = vrClient.SavedIDs["route"];
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
                                { "routeid", _routeId }, { "nodeid", bikeId }, {"\"_speed_\"", $"{0}"}
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
        
  /// <summary>
  /// In a while-loop the method UpdateFollowRoute() gets called once the routeId and bikeId are found
  /// The thread sleeps every 100 milliseconds after executing the method
  /// </summary>
    public void RunController()
    {
        while (true)
        {
            if (!String.IsNullOrEmpty(_routeId) && !String.IsNullOrEmpty(bikeId))
            {
                UpdateFollowRoute();
            }
            else
            {
                // Console.WriteLine($"Waiting for routeId and/or bikeId");
            }
            Thread.Sleep(100);
            
        }
    }

    /// <summary>
    /// The method retrieves bike data and converts it to km/h
    /// Checks whether current bike speed differs between previous speed and updates the value
    /// todo: only update VR engine when either the bike data is sent to the client
    /// todo: or only when the speed changes significantly
    ///
    /// Then the bike speed is copied to animationSpeed and followSpeed and modified to feel realistic in VR
    /// animationSpeed: speed of the 3D animation
    /// followSpeed: speed of the bike in VR
    /// </summary>
    private void UpdateFollowRoute()
    {
        //Retrieve bike data (speed)
        var bikeData = Program.GetBikeData();
        var speedRaw = bikeData[DataType.Speed].ToString(CultureInfo.InvariantCulture);
        var bikeSpeed = 0.0;
        try
        {
            bikeSpeed = 3.6 * Double.Parse(speedRaw.Substring(0, speedRaw.IndexOf('.') + 2));
        }
        catch (Exception e)
        {
            Console.WriteLine("The string speedRaw was not able to be parsed to a double.");
        }

        //If the new bikeSpeed has changed compared to the previous value, update previousSpeed
        // otherwise do not update speed in VR engine
        if (Math.Abs(bikeSpeed - previousSpeed) > 0.05)
        {
            previousSpeed = bikeSpeed;
        }
        else
        {
            return;
        }

        //Modify the animation speed based on bike speed
        var animationSpeed = 0.0 + bikeSpeed / 36;
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Route\\AnimationSpeed",
                    new Dictionary<string, string>()
                    {
                        { "nodeid", bikeId }, {"\"_speed_\"", $"{animationSpeed.ToString(CultureInfo.InvariantCulture)}"}
                    })
            }
        });
        
        //Modify the route follow speed based on bike speed
        var followSpeed = 0.0 + bikeSpeed / 2;
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Route\\FollowSpeed",
                    new Dictionary<string, string>()
                    {
                        { "nodeid", bikeId }, {"\"_speed_\"", $"{followSpeed.ToString(CultureInfo.InvariantCulture)}"}
                    })
            }
        });
    }
}