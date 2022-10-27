using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using ClientApplication;
using ClientApplication.Bike;
using ClientApplication.Util;
using ClientSide.VR;
using ClientSide.VR2.CommandHandler;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Log;

namespace ClientSide.VR2;

public class BikeController
{
    private VRClient client;
    private Tunnel tunnel;
    private WorldGen worldGen;

    private string cameraId;
    private string bikeId;

    private double previousSpeed;

    public BikeController(VRClient client, Tunnel tunnel, WorldGen worldGen)
    {
        this.client = client;
        this.tunnel = tunnel;
        this.worldGen = worldGen;
    }

    public async void Setup()
    {
        var serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {
                "\"_data_\"",
                JsonFileReader.GetObjectAsString("AddBike", new Dictionary<string, string>(), JsonFolder.Route.Path)
            },
            { "_serial_", serial }
        });

        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 1000);

        cameraId = await client.FindObjectUuid("Camera");
        Logger.LogMessage(LogImportance.DebugHighlight, "CameraId: " + cameraId);
        bikeId = await client.FindObjectUuid("bike");

        serial = Util.RandomString();
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("UpdateCamera", new Dictionary<string, string>()
                {
                    { "_guid_", cameraId }, { "_parent_", bikeId },
                    { "_serial_", serial }
                }, JsonFolder.Panel.Path)
            }
        });

        worldGen.route = await FetchRouteSubPoints();

        BikeHandler handler = App.GetBikeHandlerInstance();
        handler.Subscribe(DataType.Speed, speedRaw =>
        {
            var bikeSpeed = 3.6 * Math.Round(speedRaw, 2);

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
                {
                    "\"_data_\"", JsonFileReader.GetObjectAsString("AnimationSpeed", new Dictionary<string, string>()
                    {
                        { "nodeid", bikeId },
                        { "\"_speed_\"", $"{animationSpeed.ToString(CultureInfo.InvariantCulture)}" }
                    }, JsonFolder.Route.Path)
                }
            }, true);

            //Modify the route follow speed based on bike speed
            var followSpeed = 0.0 + bikeSpeed / 2;
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {
                    "\"_data_\"", JsonFileReader.GetObjectAsString("FollowSpeed", new Dictionary<string, string>()
                    {
                        { "nodeid", bikeId },
                        { "\"_speed_\"", $"{followSpeed.ToString(CultureInfo.InvariantCulture)}" },
                    }, JsonFolder.Route.Path)
                }
            }, false);
        });

        await worldGen.GenerateDecoration();
    }

    private async Task<List<Vector2>> FetchRouteSubPoints()
    {
        var fullRoute = new List<Vector2>();
        var serial = Util.RandomString();

        //Start following route
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {
                "\"_data_\"",
                JsonFileReader.GetObjectAsString("FollowRoute",
                    new Dictionary<string, string>
                    {
                        { "routeid", worldGen.routeId },
                        { "nodeid", bikeId },
                        { "\"_speed_\"", "50" },
                    }, JsonFolder.Route.Path)
            },
            { "_serial_", serial }
        });
        await client.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 1000);

        var firstPoint = Vector2.Zero;
        var isFirst = true;
        var hasCompletedLap = false;
        //Start fetching points
        for (var i = 0; i < 250; i++)
        {
            //Send find request
            serial = Util.RandomString();
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {
                    "\"_data_\"",
                    JsonFileReader.GetObjectAsString("Find",
                        new Dictionary<string, string>
                        {
                            { "_name_", "bike" },
                        }, JsonFolder.TunnelMessages.Path)
                },
                { "_serial_", serial }
            });

            //Read response
            await client.AddSerialCallbackTimeout(serial, ob =>
            {
                if (ob["status"]!.ToObject<string>()!.Equals("ok"))
                {
                    var pos = ob["data"]![0]!["components"]![0]!["position"]!;
                    var point = new Vector2(float.Parse(pos[0]!.ToString()), float.Parse(pos[2]!.ToString()));

                    if (isFirst)
                    {
                        isFirst = false;
                        firstPoint = point;
                    }
                    else if (Vector2.Distance(point, firstPoint) < 1) hasCompletedLap = true;
                    else fullRoute.Add(point);
                }
            }, () => { }, 1000);

            if (hasCompletedLap)
            {
                break;
            }
        }
        //
        // tunnel.SendTunnelMessage(new Dictionary<string, string>()
        // {
        //     {
        //         "\"_data_\"",
        //         JsonFileReader.GetObjectAsString("FollowRoute",
        //             new Dictionary<string, string>
        //             {
        //                 { "routeid", worldGen.routeId },
        //                 { "nodeid", bikeId },
        //                 { "\"_speed_\"", "0" },
        //             }, JsonFolder.Route.Path)
        //     },
        //     { "_serial_", serial }
        // }, true);
        // //
        // tunnel.SendTunnelMessage(new Dictionary<string, string>()
        // {
        //     {
        //         "\"_data_\"", JsonFileReader.GetObjectAsString("AnimationSpeed", new Dictionary<string, string>()
        //         {
        //             { "nodeid", bikeId },
        //             { "\"_speed_\"", "0" }
        //         }, JsonFolder.Route.Path)
        //     }
        // }, true);

        return fullRoute;
    }
}