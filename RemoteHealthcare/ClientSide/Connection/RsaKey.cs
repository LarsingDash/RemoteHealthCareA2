using ClientApplication.ServerConnection.Helpers;
using Newtonsoft.Json.Linq;
using Shared;

namespace ClientApplication.ServerConnection;

public class RsaKey : ICommandHandler
{
    /// <summary>
    /// It sends the client the public RSA key of the server
    /// </summary>
    /// <param name="Client">The client that sent the command</param>
    /// <param name="JObject">The JSON object that was sent from the client.</param>
    public void HandleCommand(ClientV2 client, JObject ob)
    {
        var dict = new Dictionary<string, string>
        {
            {"\"_key_\"", client.GetRsaPublicKey()},
        };
        if(ob.ContainsKey("serial"))
            dict.Add("_serial_", ob["serial"]?.ToObject<string>() ??"_serial_");
        client.SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", dict, JsonFolder.Json.Path));
    }
}