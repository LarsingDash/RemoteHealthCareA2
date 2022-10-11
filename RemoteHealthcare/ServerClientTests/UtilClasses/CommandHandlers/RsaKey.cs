using Newtonsoft.Json.Linq;
using Shared;
using Shared.Log;

namespace ServerClientTests.UtilClasses.CommandHandlers;

internal class RsaKey : ICommandHandler
{
    /// <summary>
    /// It sends the client the public key of the server
    /// </summary>
    /// <param name="DefaultClientConnection">The client that sent the command</param>
    /// <param name="JObject">The Json object that was sent to the server.</param>
    public void HandleCommand(DefaultClientConnection client, JObject ob)
    {
        var t = Util.ByteArrayToString(client.GetRsaPublicKey());
        var dict = new Dictionary<string, string>
        {
            {"\"_key_\"", Util.ByteArrayToString(client.GetRsaPublicKey())},
        };
        if(ob.ContainsKey("serial"))
            dict.Add("_serial_", ob["serial"]?.ToObject<string>() ??"_serial_");
        client.SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", dict, JsonFolder.Json.Path));
    }
    
}