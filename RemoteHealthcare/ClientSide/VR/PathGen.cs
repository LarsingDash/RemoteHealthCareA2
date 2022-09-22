using Newtonsoft.Json.Linq;

namespace ClientSide.VR
{
    static class PathGen
    {
        private static Dictionary<int[], int[]> Points { get; } = new Dictionary<int[], int[]>();
        private static string RouteUUID { get; set; }

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


        // RenderRoad acts on the AddRoute() callback to render a road on the route
        //The current code used duplicate code from Tunnel.sendTunnelMessage()
        //because the current structure doesn't allow easy access from the VRClient and other classes to the response data 

        //TODO: fix code structure for easy callbacks
        public static void RenderRoad(VRClient vrClient, JObject response)
        {
            RouteUUID = response["data"]["data"]["uuid"].ToObject<string>();
           
            // rewrite AddRoad.json with new values (in this case only route uuid)
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\AddRoad", new Dictionary<string, string>(){{"route uuid", RouteUUID.ToString()}
                })},     };

            // add session id for tunneling between client and server
            values.Add("_tunnelID_", vrClient.tunnelID);
            // send the data via the client
            vrClient.SendData(JsonFileReader.GetObjectAsString("SendTunnel", values));


        }
    }
}
