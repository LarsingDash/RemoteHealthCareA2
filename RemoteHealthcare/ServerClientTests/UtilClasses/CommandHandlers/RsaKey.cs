using Newtonsoft.Json.Linq;
using Shared;
using Shared.Log;

namespace ServerClientTests.UtilClasses.CommandHandlers;

internal class RsaKey : ICommandHandler
{
    /// <summary>
    /// It sends the client the public key of the server
    /// </summary>
    /// <param name="client">The client that sent the command</param>
    /// <param name="ob">The Json object that was sent to the server.</param>
    public void HandleCommand(DefaultClientConnection client, JObject ob)
    {
        var t = client.GetRsaPublicKey();
        var dict = new Dictionary<string, string>
        {
            {"_key_", client.GetRsaPublicKey()},
        };
        if(ob.ContainsKey("serial"))
            dict.Add("_serial_", ob["serial"]?.ToObject<string>() ??"_serial_");
        client.SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", dict, JsonFolder.Json.Path));
    }
    
}