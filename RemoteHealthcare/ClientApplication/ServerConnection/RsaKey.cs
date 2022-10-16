using System.Collections.Generic;
using ClientApplication.Util;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Log;

namespace ClientApplication.ServerConnection;

public class RsaKey : ICommandHandler
{
    /// <summary>
    /// It sends the client the public RSA key of the server
    /// </summary>
    /// <param name="client">The client that sent the command</param>
    /// <param name="ob">The JSON object that was sent from the client.</param>
    public void HandleCommand(Client client, JObject ob)
    {
        var dict = new Dictionary<string, string>
        {
            {"_key_", client.GetRsaPublicKey()},
        };
        if(ob.ContainsKey("serial"))
            dict.Add("_serial_", ob["serial"]?.ToObject<string>() ??"_serial_");
        client.SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", dict, JsonFolder.ServerConnection.Path));
    }
}