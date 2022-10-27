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

        public List<Vector2> route = new List<Vector2>();

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

                        var value = noiseGen.GetPerlin(x * 2, y * 2) * terrainSensitivity +
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
                            { "_diffuse_", $"data/NetworkEngine/terrain/{vrClient.terrainD}.jpg" },
                            { "_normal_", $"data/NetworkEngine/terrain/{vrClient.terrainN}.jpg" }
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
        private async Task PathGen()
        {
            try
            {
                var chosenPath = ChoosePath();

                var polyBuilder = new StringBuilder();
                foreach (var point in chosenPath)
                {
                    route.Add(new Vector2(point.X, point.Y));
                    polyBuilder.Append(PointConverter(point));
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
                                { "_uuid_", routeId },
                                {"_diffuse_", $"data/NetworkEngine/path/{vrClient.path}.jpg"}
                    }, JsonFolder.Route.Path)
                    },
                });
            }
            catch (Exception e)
            {
                Logger.LogMessage(LogImportance.Error, "No response from VR server when requesting scene/get", e);
            }
        }

        public async Task GenerateDecoration()
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
                var treesList = new List<Vector2>();
                var fullRoute = new List<Vector2>(route);

                for (var subdivisionFactor = 0; subdivisionFactor < 7; subdivisionFactor++)
                {
                    var currentList = new List<Vector2>(fullRoute);

                    for (var i = 0; i < fullRoute.Count; i++)
                    {
                        var currentPoint = fullRoute[i];
                        var nextPoint = fullRoute[(i + 1) % fullRoute.Count];

                        var subPoint = (currentPoint + nextPoint) / 2;
                        currentList.Insert(fullRoute.IndexOf(currentPoint) + 1 + i, subPoint);
                    }

                    fullRoute = currentList;
                }

                //Start decorationGen
                var maxAmountOfObjects = vrClient.decoAmount;
                const int maxFailedAttempts = 250;
                var amountOfObjects = 0;
                var failedAttempts = 0;
                var random = new Random();

                //Keep attempting to spawn trees till the max amount of trees or max amount of fails have been reached
                while (amountOfObjects < maxAmountOfObjects && failedAttempts < maxFailedAttempts)
                {
                    var attemptFailed = false;
                    var currentPoint = new Vector2(random.Next(0, mapSize * 2) - mapSize,
                        random.Next(0, mapSize * 2) - mapSize);
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
                                    // { "\"_scale_\"", (random.NextDouble()).ToString(CultureInfo.InvariantCulture) },
                                    { "\"_scale_\"", vrClient.scale },
                                    { "_filename_", $"data/NetworkEngine/decoration/{vrClient.decoration}/object.obj" }
                                }, JsonFolder.TunnelMessages.Path)
                        },
                    }, true);

                    treesList.Add(currentPoint);
                    amountOfObjects++;
                }
            }
            catch (Exception e)
            {
                Logger.LogMessage(LogImportance.Error, "Error Unknown Reason", e);
            }
        }

        private List<Vector4> ChoosePath()
        {
            if (VRClient.selectedRoute == 6)
            {
                var random = new Random();
                VRClient.selectedRoute = random.Next(0, 5);
            }

            List<Vector4> chosenPath;
            float scale;

            switch (VRClient.selectedRoute)
            {
                default:
                    scale = 4;
                    chosenPath = new List<Vector4>
                    {
                        new Vector4(3, 2, -1, 1),
                        new Vector4(-1, 1, -1, -1),
                        new Vector4(-3, 1, -1, -1),
                        new Vector4(-2, -2, 1, 1),
                        new Vector4(2, -2, 2, 2)
                    };
                    break;

                case 1:
                    scale = 5;
                    chosenPath = new List<Vector4>
                    {
                        new Vector4(3, 0, 0.5f, -0.5f),
                        new Vector4(3, -3, -1, -1),
                        new Vector4(-4, -3, -1, 1),
                        new Vector4(-4, 4, 1, 1),
                        new Vector4(1, 4, 1, -1),
                        new Vector4(1, 0, 0.5f, -0.5f),
                    };
                    break;
                
                case 2:
                    scale = 2;
                    chosenPath = new List<Vector4>
                    {
                        new Vector4(0, -2, -1, 0),
                        new Vector4(-2, 2, 1, -1),
                        new Vector4(2, 2, 1, 1),
                    };
                    break;
                
                case 3:
                    scale = 4.5f;
                    chosenPath = new List<Vector4>
                    {
                        new Vector4(4, 2, 1, -1),
                        new Vector4(3, -2, -1, -1),
                        new Vector4(-1, -4, -1, 1),
                        new Vector4(-4, -1, 0, 1),
                        new Vector4(-1, 3, 1, 1),
                    };
                    break;
                
                case 4:
                    scale = 5;
                    chosenPath = new List<Vector4>
                    {
                        new Vector4(1, -4, -1, 1),
                        new Vector4(-2, -1, -1, 1),
                        new Vector4(-4, 2, 0, 1),
                        new Vector4(-2, 4, 1, 1),
                        new Vector4(2, 4, 1, -1),
                        new Vector4(4, 0, 1, -1),
                        new Vector4(3, -3, 1, -1),
                    };
                    break;
                
                case 5:
                    scale = 4;
                    chosenPath = new List<Vector4>
                    {
                        new Vector4(3, 4, 1, -1),
                        new Vector4(3, 0, -1, -1),
                        new Vector4(-1, -2, -1, 0),
                        new Vector4(-4, 0, -1, 1),
                        new Vector4(-3, 3, 1, 1),
                    };
                    break;
            }

            var finalPath = new List<Vector4>();
            const float curveFactor = 5;
            foreach (var point in chosenPath)
            {
                var currentPoint = point;
                currentPoint.Z *= curveFactor;
                currentPoint.W *= curveFactor;
                finalPath.Add(currentPoint / scale * (mapSize / 2 - 45));
            }

            return finalPath;
        }

        private string PointConverter(Vector4 point)
        {
            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append(
                $"\"pos\": [{point.X.ToString(CultureInfo.InvariantCulture)}, 0, {point.Y.ToString(CultureInfo.InvariantCulture)}],");
            builder.Append(
                $"\"dir\": [{point.Z.ToString(CultureInfo.InvariantCulture)}, 0, {point.W.ToString(CultureInfo.InvariantCulture)}]");
            builder.Append("}");

            return builder.ToString();
        }
    }
}