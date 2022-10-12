using System.Globalization;
using System.Numerics;
using System.Text;
using ClientSide.Helpers;
using ClientSide.VR2;
using DoctorApplication.Communication.CommandHandlers;
using Shared;
using Shared.Log;

namespace ClientSide.VR
{
    /// <summary>
    /// Manages terrain and route generation as well object placement
    /// </summary>
    public class WorldGen
    {
        private readonly VRClient vrClient;
        private readonly Tunnel tunnel;

        private const int mapSize = 256;
        private double[,] heights = new double[256, 256];
        public string routeId;

        private List<string> treePath = new()
        {
            "data/NetworkEngine/models/trees/fantasy/tree7.obj",
            "data/NetworkEngine/models/trees/fantasy/tree6.obj",
        };
            
        // private const string treePath = "data/NetworkEngine/models/houses/set1/house1.obj";

        private List<Vector2> route = new();

        public WorldGen(VRClient vrClient, Tunnel tunnel)
        {
            this.vrClient = vrClient;
            this.tunnel = tunnel;

            GenerateTerrain();
        }

        private async void GenerateTerrain()
        {
            try
            {
                //Set height values for tiles
                var noiseGen = new DotnetNoise.FastNoise();
                var heightMap = new StringBuilder();

                //Determines sensitivity of the terrain height. Higher values equal to higher height difference
                var terrainSensitivity = 10;

                for (var x = 0; x < mapSize; x++)
                {
                    for (var y = 0; y < mapSize; y++)
                    {
                        heights[x, y] = (noiseGen.GetPerlin(x, y) * terrainSensitivity);
                        heightMap.Append(
                            $"{(noiseGen.GetPerlin(x, y) * terrainSensitivity).ToString(CultureInfo.InvariantCulture)},");
                    }
                }

                heightMap.Remove(heightMap.Length - 1, 1);

                //Plan to add grass to terrain
                var serial = Util.RandomString();
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"", JsonFileReader.GetObjectAsString("AddTerrain", new Dictionary<string, string>
                        {
                            {"\"_size1_\"", $"{mapSize}"},
                            {"\"_size2_\"", $"{mapSize}"},
                            {"\"_heights_\"", heightMap.ToString()}
                        }, JsonFolder.Terrain.Path)
                    },
                    {"_serial_", serial}
                });
                //TODO Check status etc?
                await vrClient.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 1000);

                serial = Util.RandomString();
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"",
                        JsonFileReader.GetObjectAsString("AddNodeTerrain", new Dictionary<string, string>(),
                            JsonFolder.Terrain.Path)
                    },
                    {"_serial_", serial}
                });
                //TODO Check status etc?
                await vrClient.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 1000);

                string terrainId = await vrClient.FindObjectUuid("terrain");
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"", JsonFileReader.GetObjectAsString("AddLayer", new Dictionary<string, string>
                        {
                            {"_uuid_", terrainId},
                            {"_diffuse_", "data/NetworkEngine/textures/terrain/grass_green2y_d.jpg"},
                            {"_normal_", "data/NetworkEngine/textures/terrain/grass_green2y_n.jpg"}
                        }, JsonFolder.Terrain.Path)
                    },
                });

                await PathGen();

                //Add terrain
            }
            catch (Exception e)
            {
                Logger.LogMessage(LogImportance.Warn, "No response from VR server when requesting scene/get");
            }

        }

        //Prepare road and send route
        public async Task PathGen()
        {
            try
            {
                var poly = GenPoly(101, 100, 20, 25, new Random());
                route.AddRange(poly);

                var polyBuilder = new StringBuilder();
                for (int i = 0; i < poly.Length; i++)
                {
                    polyBuilder.Append(PointConverter(poly[i], poly[(i + 1) % poly.Length]));
                    polyBuilder.Append(",");
                }

                polyBuilder.Remove(polyBuilder.Length - 1, 1);

                var serial = Util.RandomString();
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"",
                        JsonFileReader.GetObjectAsString("AddRoute",
                            new Dictionary<string, string>
                            {
                                {"\"_nodes_\"", polyBuilder.ToString()}
                            }, JsonFolder.Route.Path)
                    },
                    {"_serial_", serial}
                });
                routeId = "";
                vrClient.BikeController.Setup();
                
                await vrClient.AddSerialCallbackTimeout(serial, ob =>
                {
                    if (ob["status"]!.ToObject<string>()!.Equals("ok"))
                    {
                        routeId = ob["data"]!["uuid"]!.ToObject<string>()!;
                    }
                }, () =>
                {
                    //TODO Road could not be made
                }, 1000);
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"", JsonFileReader.GetObjectAsString("AddRoad",
                            new Dictionary<string, string>()
                            {
                                {"_uuid_", routeId}
                            }, JsonFolder.Route.Path)
                    },
                });


                new Thread((o) => { GenerateDecoration(); }).Start();
            }
            catch (Exception e)
            {
                Logger.LogMessage(LogImportance.Warn, "No response from VR server when requesting scene/get");
            }
        }

        private async Task GenerateDecoration()
        {
            try
            {
                var serial = Util.RandomString();
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"",
                        JsonFileReader.GetObjectAsString("AddNodeScene",
                            new Dictionary<string, string>
                            {
                                {"_name_", "trees"}
                            }, JsonFolder.TunnelMessages.Path)
                    },
                    {"_serial_", serial}
                });

                string treesId = "";
                await vrClient.AddSerialCallbackTimeout(serial, ob =>
                {
                    if (ob["status"]!.ToObject<string>()!.Equals("ok"))
                    {
                        treesId = ob["data"]!["uuid"]!.ToObject<string>()!;
                    }
                }, () => { }, 1000);

                Logger.LogMessage(LogImportance.Debug, $"Treesid: {treesId}");
                //Subdivide the route to get more sub-points
                Console.WriteLine("Starting subdivision");
                var fullRoute = new List<Vector2>();
                for (var i = 0; i < route.Count; i++)
                {
                    var currentPoint = route[i];
                    var nextPoint = route[(i + 1) % route.Count];

                    var subPoint = (currentPoint + nextPoint) / 2;
                    fullRoute.Add(subPoint);
                }

                Console.WriteLine("Subdivision completed");

                const int maxAmountOfObjects = 50;
                //const int maxFailedAttempts = 10;
                var amountOfObjects = 0;
                var failedAttempts = 0;
                Random random = new Random();
                Console.WriteLine("Trees sent:");
                for (int i = 0; i < maxAmountOfObjects; i++)
                {
                    new Thread(async start =>
                    {

                        var currentPoint = new Vector2(random.Next(0, 256), random.Next(0, 256));
                        serial = Util.RandomString();
                        tunnel.SendTunnelMessage(new Dictionary<string, string>()
                        {
                            {
                                "\"_data_\"", JsonFileReader.GetObjectAsString("GetHeight",
                                    new Dictionary<string, string>()
                                    {
                                        {"_serial_", serial},
                                        {"\"_pos_\"", $"{currentPoint.X}, {currentPoint.Y}"}
                                    }, JsonFolder.Terrain.Path)
                            }
                        });
                        string height = "0";
                        await vrClient.AddSerialCallbackTimeout(serial,
                            ob => { height = ob["data"]!["height"]!.ToObject<string>()!; },
                            () => { Logger.LogMessage(LogImportance.Fatal, "No response"); }, 1000);

                        tunnel.SendTunnelMessage(new Dictionary<string, string>()
                        {
                            {
                                "\"_data_\"", JsonFileReader.GetObjectAsString("AddModel",
                                    new Dictionary<string, string>
                                    {
                                        {"_name_", $"tree{amountOfObjects}"},
                                        {"_guid_", treesId},
                                        {
                                            "\"_position_\"",
                                            $"{currentPoint.X + ".0"} , {height}, {currentPoint.Y + ".0"}"
                                        },

                                        // { "\"_position_\"", "0, 0, 0" },
                                        {"_filename_", treePath[random.Next(treePath.Count)]}
                                    }, JsonFolder.TunnelMessages.Path)
                            },
                        });
                        amountOfObjects++;
                        Logger.LogMessage(LogImportance.Debug, $"Amount of Trees placed: {amountOfObjects}");
                    }).Start();
                    await Task.Delay(10);
                }
            }
            catch (Exception e)
            {
                Logger.LogMessage(LogImportance.Warn, "No response from VR server when requesting scene/get");
            }
        }

        private Vector2[] GenPoly(double RadiusMin, double RadiusMax, int minPoints, int maxPoints, Random random)
        {
            //Choose the amount of points
            var amountOfPoints = random.Next(minPoints, maxPoints);
            var points = new Vector2[amountOfPoints];

            //Determine the angle between the points
            var angle = (float)(Math.PI * 2) / amountOfPoints;
            for (var i = 0; i < amountOfPoints; i++)
            {
                //Generate each point using some variety between each points
                var RadiusUse = (float)(random.NextDouble() / 10 * (RadiusMax - RadiusMin) + RadiusMin);
                var currentAngle = angle * i;
                var currentPoint = new Vector2(
                    (int)(Math.Sin(currentAngle) * RadiusUse),
                    (int)(Math.Cos(currentAngle) * RadiusUse));

                points[i] = currentPoint;
            }

            return points;
        }

        private string PointConverter(Vector2 point, Vector2 nextPoint)
        {
            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append($"\"pos\": [{point.X}, 0, {point.Y}],");

            // string dir;
            // var horNegative = nextPoint.X < point.X;
            // var verNegative = nextPoint.Y < point.Y;
            //
            // var scaleRaw = Math.Sqrt(Math.Pow(point.X - nextPoint.X, 2) + Math.Pow(point.Y - nextPoint.Y, 2));
            // var scaleString = scaleRaw.ToString(CultureInfo.InvariantCulture);
            // var scale = scaleString.Substring(0, scaleString.IndexOf('.') + 2);
            //
            // switch (horNegative)
            // {
            //     default:
            //     case true when verNegative:
            //         dir = $"-{scale}, 0, -{scale}";
            //         break;
            //     case false when verNegative:
            //         dir = $"{scale}, 0, -{scale}";
            //         break;
            //     case false when !verNegative:
            //         dir = $"{scale}, 0, {scale}";
            //         break;
            //     case true when !verNegative:
            //         dir = $"-{scale}, 0, {scale}";
            //         break;
            // }

            // builder.Append($"\"dir\": [{dir}]");

            builder.Append($"\"dir\": [{nextPoint.X - point.X}, 0, {nextPoint.Y - point.Y}]");
            builder.Append("}");


            return builder.ToString();
        }
    }
}