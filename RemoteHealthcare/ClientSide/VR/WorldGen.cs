using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    GenerateRoad();
                    generateForest();
                    break;
            }
        }

        private void GenerateRoad()
        {
            //Generate Route
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\AddRoute", new Dictionary<string, string>(){})},
            });
        }

        private void generateForest()
        {
            //Set height values for tiles
            var noiseGen = new DotnetNoise.FastNoise();
            string heights = "";
            for (float x = 0; x < 256; x++)
            {
                for (float y = 0; y < 256; y++)
                {
                    heights += "0,";
                }
            }

            heights = heights.Substring(0, heights.Length - 1);

            //Add terain
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\AddTerrain", new Dictionary<string, string>()
                {
                    {"\"_size1_\"", "256"},
                    {"\"_size2_\"", "256"},
                    {"\"_heights_\"", heights}
                })},
            });


            //Add trees
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Tree", new Dictionary<string, string>(){})},
            });
        }
    }

    public enum World
    {
        forest
    }
}
