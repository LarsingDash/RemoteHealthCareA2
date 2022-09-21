using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSide.VR
{
     static class PathGen
    {
        private static Dictionary<int[], int[]> Points { get; } = new Dictionary<int[], int[]>();
        private static string UUID { get; set; }

        //TODO: implement JSON editor
        public static void GenerateForestPath()
        {

            // Points.Add(new int[] {0, 0, 0}, new int[] {5, 0, -5});
            // Points.Add(new int[] {50, 0, 0}, new int[] {5, 0, 5});
            // Points.Add(new int[] {50, 0, 50}, new int[] {-5, 0, 5});
            // Points.Add(new int[] {0, 0, 50}, new int[] {-5, 0, -5});
            //Generate Route
           

           // return Points;
        }




        public static void RenderRoad(VRClient vrClient, JObject response)
        {
            UUID = response["data"]["data"]["uuid"].ToObject<string>();

            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\AddRoad", new Dictionary<string, string>(){{"route uuid", UUID.ToString()}
                })},     };

            values.Add("_tunnelID_", vrClient.tunnelID);
            vrClient.SendData(JsonFileReader.GetObjectAsString("SendTunnel", values));


        }
    }
}
