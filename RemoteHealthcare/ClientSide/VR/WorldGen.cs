using System.Drawing;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ClientSide.VR
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
            for (var x = 0; x < mapSize; x++)
            {
                for (var y = 0; y < mapSize; y++)
                {
                    var fullValue = noiseGen.GetPerlin(x, y) * 100;
                    var roundedValue = (int)(fullValue * 100);
                    var value = roundedValue / 1000;
                    heightMap.Append($"{value},");
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
                polyBuilder.Append(pointConverter(poly[i], poly[(i + 1) % poly.Length]));
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
                            {"\"_nodes_\"", polyBuilder.ToString()}
                        })
                }
            });
        }

        //Prepare bike and add bike to scene
        public void AnimateBike()
        {
            string nodeName = "bike";
            string routeId = "";

            // After adding the route, prepare the to-be-added bike for following route
            vrClient.IDWaitList.Add(nodeName, nodeId =>
            {
                // Retrieve routeId that was added
                if (vrClient.SavedIDs.ContainsKey("route"))
                {
                    routeId = vrClient.SavedIDs["route"];
                }

                // check if routeid is saved and let bike follow route
                if (!String.IsNullOrEmpty(routeId))
                {
                    tunnel.SendTunnelMessage(new Dictionary<string, string>()
                    {
                        {
                            "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Route\\FollowRoute",
                                new Dictionary<string, string>()
                                {
                                    { "routeid", routeId }, { "nodeid", nodeId.ToString() }
                                })
                        },
                    });
                }
            });

            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {
                    "\"_data_\"",
                    JsonFileReader.GetObjectAsString("TunnelMessages\\Route\\AddBike", new Dictionary<string, string>())
                },
            });
        }
        
        private Point[] GenPoly(double RadiusMin,double RadiusMax,int minPoints,int maxPoints,Random random)
        {
            //select a number of points in the given range.
            int PointCount = random.Next(minPoints, maxPoints);
            var PolyPoints = new Point[PointCount];
            
            //calculate the angle between each generated point.
            float Angle = (float)(Math.PI * 2) / PointCount;
            for (int i = 0; i < PointCount; i++)
            {
                //generate this point, generating a radius between the given minimum and maximum range.
                float RadiusUse = (float)((random.NextDouble() * (RadiusMax - RadiusMin)) + RadiusMin);
                float useangle =Angle*i;
                Point newPoint = new Point((int)(Math.Sin(useangle) * RadiusUse),(int)(Math.Cos(useangle) * RadiusUse));

                PolyPoints[i] = newPoint;
            }
            return PolyPoints;
        }

        private string pointConverter(Point point, Point nextPoint)
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