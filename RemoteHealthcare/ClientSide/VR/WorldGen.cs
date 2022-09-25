using System.Text;

namespace ClientSide.VR
{
    public class WorldGen
    {
        Tunnel tunnel;
        public WorldGen(Tunnel tunnel, World selectedWorld)
        {
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
                    var value = (int)(noiseGen.GetPerlin(x, y) * 100) / 10f;
                    heightMap.Append($"{value},");
                }
            }
            heightMap.Remove(heightMap.Length - 1, 1);
            
            //Add terrain
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\AddTerrain", new Dictionary<string, string>
                {
                    {"\"_size1_\"", $"{mapSize}"},
                    {"\"_size2_\"", $"{mapSize}"},
                    {"\"_heights_\"", heightMap.ToString()}
                })},
            });
            
            //Add root node
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\AddNodeScene", new Dictionary<string, string>
                {
                    {"_name_", "terrain"}
                })},
            });

            // //Finding terrain to add texture to
            // tunnel.SendTunnelMessage(new Dictionary<string, string>()
            // {
            //     {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\AddLayer", new Dictionary<string, string>
            //     {
            //         {"\"_uuid_\"", "256"}
            //     })},
            // });
            //
            // //Add terrain texture
            // tunnel.SendTunnelMessage(new Dictionary<string, string>()
            // {
            //     {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\AddLayer", new Dictionary<string, string>()
            //     {
            //         {"\"_uuid_\"", "256"}
            //     })},
            // });
        }
    }

    public enum World
    {
        forest
    }
}
