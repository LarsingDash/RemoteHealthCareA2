using System.Globalization;
using ClientSide.Bike;
using ClientSide.Helpers;
using ClientSide.VR2.CommandHandler;
using DoctorApplication.Communication.CommandHandlers;
using Microsoft.VisualBasic.FileIO;
using Shared;
using Shared.Log;

namespace ClientSide.VR2;

public class BikeController
{
    private VRClient client;
    private Tunnel tunnel;

    private string cameraId;
    private string bikeId;
    private string routeId;
    
    private double previousSpeed;
    
    public BikeController(VRClient client, Tunnel tunnel)
    {
        this.client = client;
        this.tunnel = tunnel;
    }

    public async void Setup()
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("AddBike", new Dictionary<string, string>(), JsonFolder.Route.Path)},
            {"_serial_", serial}
        });

        await client.AddSerialCallbackTimeout(serial, ob =>
        {}, () => {}, 1000);

        cameraId = await client.FindObjectUuid("Camera");
        Logger.LogMessage(LogImportance.DebugHighlight, "CameraId: " + cameraId);
        bikeId = await client.FindObjectUuid("bike");

        serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("FollowRoute", new Dictionary<string, string>()
                {
                    { "routeid", client.worldGen.routeId }, { "nodeid", bikeId }, {"\"_speed_\"", $"{0}"},
                    {"_serial_", serial}
                }, JsonFolder.Route.Path)
            }
        });
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 1000);
        
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("UpdateCamera", new Dictionary<string, string>()
                {
                    {"_guid_", cameraId}, {"_parent_", bikeId},
                    {"_serial_", serial}
                }, JsonFolder.Panel.Path)
                
            }
        });
        BikeHandler handler = Program.handler;
        handler.Subscribe(DataType.Speed, speedRaw =>
        {
            var bikeSpeed = 3.6 * Math.Round(speedRaw,2);

            //If the new bikeSpeed has changed compared to the previous value, update previousSpeed
            // otherwise do not update speed in VR engine
            if (Math.Abs(bikeSpeed - previousSpeed) <= 0.05)
            {
                return;
            }
            previousSpeed = bikeSpeed;

            //Modify the animation speed based on bike speed
            var animationSpeed = 0.0 + bikeSpeed / 36;
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("AnimationSpeed", new Dictionary<string, string>()
                    {
                        { "nodeid", bikeId }, {"\"_speed_\"", $"{animationSpeed.ToString(CultureInfo.InvariantCulture)}"}
                    }, JsonFolder.Route.Path)
                }
            });
            
            //Modify the route follow speed based on bike speed
            var followSpeed = 0.0 + bikeSpeed / 2;
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("FollowSpeed", new Dictionary<string, string>()
                    {
                        { "nodeid", bikeId }, {"\"_speed_\"", $"{followSpeed.ToString(CultureInfo.InvariantCulture)}"}
                    }, JsonFolder.Route.Path)
                }
            });
        });
    }
    
}