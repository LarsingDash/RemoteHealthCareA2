using Newtonsoft.Json.Linq;

namespace ClientSide.VR.CommandHandlers;

public class TunnelCreate : ICommandHandler
{
    /// <summary>
    /// This function is called when the client receives a response from the server
    /// </summary>
    /// <param name="VRClient">The client that sent the request</param>
    /// <param name="JObject">The JSON object that was received from the server.</param>
    public void handleCommand(VRClient client, JObject ob)
    {
        Console.WriteLine(ob.ToString());
        if(ob["data"]["status"].ToObject<string>().Equals("ok"))
        {
            client.setTunnelID(ob["data"]["id"].ToObject<string>());
        }
    }
}