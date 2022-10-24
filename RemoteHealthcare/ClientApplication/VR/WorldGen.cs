using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientApplication.Util;
using ClientSide.VR2;
using ClientSide.VR2.CommandHandler;
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
        private readonly float[,] heights = new float[mapSize, mapSize];
        public string routeId;

        private const string treePath = "data/NetworkEngine/models/trees/pine/Pine_Low-poly_1.obj";
        private readonly List<Vector2> route = new List<Vector2>();

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
                Logger.LogMessage(LogImportance.Debug, "Generating Terrain");
                //Set height values for tiles
                var noiseGen = new DotnetNoise.FastNoise();
                var heightMap = new StringBuilder();

                //Determines sensitivity of the terrain height. Higher values equal to higher height difference
                const float terrainSensitivity = 7.5f;

                for (var y = 0; y < mapSize; y++)
                {
                    for (var x = 0; x < mapSize; x++)
                    {
                        const float edgeMargin = mapSize / 10f;

                        var xEdgeOffset = 0f;
                        if (x < edgeMargin)
                        {
                            xEdgeOffset = edgeMargin - x;
                        }
                        else if (x - (mapSize - edgeMargin) > 0)
                        {
                            xEdgeOffset = x - (mapSize - edgeMargin);
                        }
                        
                        var yEdgeOffset = 0f;
                        if (y < edgeMargin)
                        {
                            yEdgeOffset = edgeMargin - y;
                        }
                        else if (y - (mapSize - edgeMargin) > 0)
                        {
                            yEdgeOffset = y - (mapSize - edgeMargin);
                        }

                        var value = noiseGen.GetPerlin(x, y) * terrainSensitivity + 
                                    (float)(Math.Pow(xEdgeOffset, 2) / (edgeMargin * 2)) +
                                    (float)(Math.Pow(yEdgeOffset, 2) / (edgeMargin * 2));

                        heights[x, y] = value;
                        heightMap.Append(
                            $"{value.ToString(CultureInfo.InvariantCulture)},");
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
                            { "\"_size1_\"", $"{mapSize}" },
                            { "\"_size2_\"", $"{mapSize}" },
                            { "\"_heights_\"", heightMap.ToString() }
                        }, JsonFolder.Terrain.Path)
                    },
                    { "_serial_", serial }
                }, true);
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
                    { "_serial_", serial }
                });
                //TODO Check status etc?
                await vrClient.AddSerialCallbackTimeout(serial, ob => { }, () => { }, 1000);

                string terrainId = await vrClient.FindObjectUuid("terrain");
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"", JsonFileReader.GetObjectAsString("AddLayer", new Dictionary<string, string>
                        {
                            { "_uuid_", terrainId },
                            { "_diffuse_", "data/NetworkEngine/textures/terrain/grass_green2y_d.jpg" },
                            { "_normal_", "data/NetworkEngine/textures/terrain/grass_green2y_n.jpg" }
                        }, JsonFolder.Terrain.Path)
                    },
                });

                await PathGen();
            }
            catch (Exception e)
            {
                Logger.LogMessage(LogImportance.Error, "No response from VR server when requesting scene/get", e);
            }
        }

        //Prepare road and send route
        public async Task PathGen()
        {
            try
            {
                var poly = GenPoly(85, 86, 23, 25, new Random());
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
                                { "\"_nodes_\"", polyBuilder.ToString() }
                            }, JsonFolder.Route.Path)
                    },
                    { "_serial_", serial }
                });
                routeId = "";
                vrClient.BikeController.Setup();
                vrClient.PanelController.Setup();
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
                                { "_uuid_", routeId }
                            }, JsonFolder.Route.Path)
                    },
                });


                new Thread((o) => { GenerateDecoration(); }).Start();
            }
            catch (Exception e)
            {
                Logger.LogMessage(LogImportance.Error, "No response from VR server when requesting scene/get", e);
            }
        }

        private async Task GenerateDecoration()
        {
            try
            {
                //Generate TreeContainerNode
                var serial = Util.RandomString();
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"",
                        JsonFileReader.GetObjectAsString("AddNodeScene",
                            new Dictionary<string, string>
                            {
                                { "_name_", "trees" }
                            }, JsonFolder.TunnelMessages.Path)
                    },
                    { "_serial_", serial }
                });

                //Await creation of treesId
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
                var treesList = new List<Vector2>(route);
                var fullRoute = new List<Vector2>(route);
                for (var subdivisionFactor = 0; subdivisionFactor < 3; subdivisionFactor++)
                {
                    for (var i = 0; i < route.Count; i++)
                    {
                        var currentPoint = route[i];
                        var nextPoint = route[(i + 1) % route.Count];

                        var difference = currentPoint - nextPoint;
                        var subPoint = currentPoint + difference * 0.5f;
                        fullRoute.Add(subPoint);
                    }
                }

                //Start decorationGen
                const int maxAmountOfObjects = 5000;
                const int maxFailedAttempts = 250;
                var amountOfObjects = 0;
                var failedAttempts = 0;
                var random = new Random();
                
                //Keep attempting to spawn trees till the max amount of trees or max amount of fails have been reached
                while (amountOfObjects < maxAmountOfObjects && failedAttempts < maxFailedAttempts)
                {
                    var attemptFailed = false;
                    var currentPoint = new Vector2(random.Next(0, mapSize * 2) - mapSize, random.Next(0, mapSize * 2) - mapSize);
                    foreach (var comparingPoint in fullRoute)
                    {
                        if (Vector2.Distance(comparingPoint, currentPoint) < 10)
                        {
                            failedAttempts++;
                            attemptFailed = true;
                            
                            break;
                        }
                    }
                    foreach (var comparingPoint in treesList)
                    {
                        if (Vector2.Distance(comparingPoint, currentPoint) < 3)
                        {
                            failedAttempts++;
                            attemptFailed = true;
                            
                            break;
                        }
                    }

                    if (attemptFailed) continue;

                    var currentHeight = 0f;
                    try
                    {
                        currentHeight = heights[(int)currentPoint.X + mapSize / 2, (int)currentPoint.Y + mapSize / 2];
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }


                    tunnel.SendTunnelMessage(new Dictionary<string, string>()
                    {
                        {
                            "\"_data_\"", JsonFileReader.GetObjectAsString("AddModel",
                                new Dictionary<string, string>
                                {
                                    { "_name_", $"tree{amountOfObjects}" },
                                    { "_guid_", treesId },
                                    {
                                        "\"_position_\"",
                                        $"{currentPoint.X + mapSize / 2 + ".0"} , {currentHeight.ToString(CultureInfo.InvariantCulture)}, {currentPoint.Y + mapSize / 2 + ".0"}"
                                    },
                                    {"\"_scale_\"", (random.NextDouble() + 4).ToString(CultureInfo.InvariantCulture)},
                                    { "_filename_", treePath }
                                }, JsonFolder.TunnelMessages.Path)
                        },
                    });
                    
                    treesList.Add(currentPoint);
                    amountOfObjects++;
                }

                //Resume the simulation
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"",
                        JsonFileReader.GetObjectAsString("Play", new Dictionary<string, string>(),
                            JsonFolder.TunnelMessages.Path)
                    },
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
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

            // builder.Append($"\"dir\": [{nextPoint.X - point.X}, 0, {nextPoint.Y - point.Y}]");
            builder.Append($"\"dir\": [0, 0, 0]");
            builder.Append("}");

            return builder.ToString();
        }
    }
}