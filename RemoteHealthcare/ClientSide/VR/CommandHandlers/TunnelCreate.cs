using Newtonsoft.Json.Linq;

namespace ClientSide.VR.CommandHandlers;

public class TunnelCreate : CommandHandler
{
    public void handleCommand(VRClient client, JObject ob)
    {
        Console.WriteLine(ob.ToString());
        if(ob["data"]["status"].ToObject<string>().Equals("ok"))
        {
            client.setTunnelID(ob["data"]["id"].ToObject<string>());
        }
    }
}