using System.Drawing;
using System.Globalization;
using System.Text;
using Shared;

namespace ClientApplication.ServerConnection.VR
{
    public class WorldGen
    {
        private readonly VRClient vrClient;
        private readonly Tunnel tunnel;

        public WorldGen(VRClient vrClient, Tunnel tunnel, World selectedWorld)
        {
            this.vrClient = vrClient;
            this.tunnel = tunnel;

            switch (selectedWorld)
            {
                default:
                case World.forest:
                    GenerateForest();
                    break;
            }
        }

        private void GenerateForest()
        {
            //Set height values for tiles
            const int mapSize = 256;
            var noiseGen = new DotnetNoise.FastNoise();
            var heightMap = new StringBuilder();

            //Determines sensitivity of the terrain height. Higher values equal to higher height difference
            var terrainSensitivity = 10;

            for (var x = 0; x < mapSize; x++)
            {
                for (var y = 0; y < mapSize; y++)
                {
                    heightMap.Append($"{(noiseGen.GetPerlin(x, y) * terrainSensitivity).ToString(CultureInfo.InvariantCulture)},");
                }
            }

            heightMap.Remove(heightMap.Length - 1, 1);

            //Plan to add grass to terrain
            vrClient.IDWaitList.Add("terrain", terrainID =>
            {
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Terrain\\AddLayer",
                            new Dictionary<string, string>
                            {
                                { "_uuid_", terrainID },
                                { "_diffuse_", "data/NetworkEngine/textures/terrain/grass_green2y_d.jpg" },
                                { "_normal_", "data/NetworkEngine/textures/terrain/grass_green2y_n.jpg" }
                            })
                    },
                }, true);
            });

            //Add terrain
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {
                    "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Terrain\\AddTerrain",
                        new Dictionary<string, string>
                        {
                            { "\"_size1_\"", $"{mapSize}" },
                            { "\"_size2_\"", $"{mapSize}" },
                            { "\"_heights_\"", heightMap.ToString() }
                        })
                },
            }, true);

            //Add root node
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {
                    "\"_data_\"",
                    JsonFileReader.GetObjectAsString("TunnelMessages\\Terrain\\AddNodeTerrain",
                        new Dictionary<string, string>())
                },
            });
        }

        //Prepare road and send route
        public void PathGen()
        {
            var poly = GenPoly(50, 80, 10, 15, new Random());

            string nodeName = "route";
            vrClient.IDWaitList.Add(nodeName, routeId =>
            {
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Route\\AddRoad",
                            new Dictionary<string, string>()
                            {
                                { "route uuid", routeId.ToString() }
                            })
                    },
                });
            });

            var polyBuilder = new StringBuilder();
            for (int i = 0; i < poly.Length; i++)
            {
                polyBuilder.Append(PointConverter(poly[i], poly[(i + 1) % poly.Length]));
                polyBuilder.Append(",");
            }

            polyBuilder.Remove(polyBuilder.Length - 1, 1);

            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {
                    "\"_data_\"",
                    JsonFileReader.GetObjectAsString("TunnelMessages\\Route\\AddRoute",
                        new Dictionary<string, string>
                        {
                            { "\"_nodes_\"", polyBuilder.ToString() }
                        })
                }
            });
        }

        //Prepare bike and add bike to scene
        public void AnimateBike()
        {
            string routeId = "";
            
            // After adding the route, prepare the to-be-added bike for following route
            vrClient.IDWaitList.Add("bike", bikeId =>
            {
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


        private Point[] GenPoly(double RadiusMin, double RadiusMax, int minPoints, int maxPoints, Random random)
        {
            //Choose the amount of points
            var amountOfPoints = random.Next(minPoints, maxPoints);
            var points = new Point[amountOfPoints];

            //Determine the angle between the points
            var angle = (float)(Math.PI * 2) / amountOfPoints;
            for (var i = 0; i < amountOfPoints; i++)
            {
                //Generate each point using some variety between each points
                var RadiusUse = (float)(random.NextDouble() * (RadiusMax - RadiusMin) + RadiusMin);
                var currentAngle = angle * i;
                var currentPoint = new Point(
                    (int)(Math.Sin(currentAngle) * RadiusUse),
                    (int)(Math.Cos(currentAngle) * RadiusUse));

                points[i] = currentPoint;
            }

            return points;
        }

        private string PointConverter(Point point, Point nextPoint)
        {
            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append($"\"pos\": [{point.X}, 0, {point.Y}],");
            builder.Append($"\"dir\": [{nextPoint.X - point.X}, 0, {nextPoint.Y - point.Y}]");
            builder.Append("}");

            return builder.ToString();
        }
    }

    public enum World
    {
        forest
    }
}