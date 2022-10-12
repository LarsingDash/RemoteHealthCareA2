using System.Globalization;
using System.Numerics;
using System.Text;
using Shared;

namespace ClientSide.VR
{
    /// <summary>
    /// Manages terrain and route generation as well object placement
    /// </summary>
    public class WorldGenOld
    {
        private readonly VrClient vrClient;
        private readonly TunnelOld tunnelOld;

        private const int mapSize = 256;

        private const string treePath = "data/NetworkEngine/models/trees/fantasy/tree7.obj";
        // private const string treePath = "data/NetworkEngine/models/houses/set1/house1.obj";

        private List<Vector2> route = new List<Vector2>();

        public WorldGenOld(VrClient vrClient, TunnelOld tunnelOld)
        {
            this.vrClient = vrClient;
            this.tunnelOld = tunnelOld;

            GenerateTerrain();
        }

        private void GenerateTerrain()
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
                    heightMap.Append(
                        $"{(noiseGen.GetPerlin(x, y) * terrainSensitivity).ToString(CultureInfo.InvariantCulture)},");
                }
            }

            heightMap.Remove(heightMap.Length - 1, 1);

            //Plan to add grass to terrain
            vrClient.IDWaitList.Add("terrain", terrainID =>
            {
                tunnelOld.SendTunnelMessage(new Dictionary<string, string>()
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
            tunnelOld.SendTunnelMessage(new Dictionary<string, string>()
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
            tunnelOld.SendTunnelMessage(new Dictionary<string, string>()
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
            var poly = GenPoly(101, 100, 20, 25, new Random());
            route.AddRange(poly);

            string nodeName = "route";
            vrClient.IDWaitList.Add(nodeName, routeId =>
            {
                tunnelOld.SendTunnelMessage(new Dictionary<string, string>()
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

            tunnelOld.SendTunnelMessage(new Dictionary<string, string>()
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

            new Thread(GenerateDecoration).Start();
        }

        private void GenerateDecoration()
        {
            tunnelOld.SendTunnelMessage(new Dictionary<string, string>()
            {
                {
                    "\"_data_\"",
                    JsonFileReader.GetObjectAsString("TunnelMessages\\AddNodeScene",
                        new Dictionary<string, string>
                        {
                            { "_name_", "trees" }
                        })
                },
            });

            
            vrClient.IDWaitList.Add("trees", treesID =>
            {
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
                const int maxFailedAttempts = 10;
                var amountOfObjects = 0;
                var failedAttempts = 0;
                Random random = new Random();
                Console.WriteLine("Trees sent:");
                while (amountOfObjects < maxAmountOfObjects && failedAttempts < maxFailedAttempts)
                {
                    var currentPoint = new Vector2(random.Next(0, 256), random.Next(0, 256));
                    tunnelOld.SendTunnelMessage(new Dictionary<string, string>()
                    {
                        {
                            "\"_data_\"",
                            JsonFileReader.GetObjectAsString("TunnelMessages\\AddModel",
                                new Dictionary<string, string>
                                {
                                    { "_name_", $"tree{amountOfObjects}" },
                                    {"_guid_", treesID},
                                    {"\"_position_\"", $"{currentPoint.X}, 0, {currentPoint.Y}"},
                                    // { "\"_position_\"", "0, 0, 0" },
                                    { "_filename_", treePath }
                                })
                        },
                    });
                    amountOfObjects++;
                    Console.WriteLine(amountOfObjects);
                }
            });
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