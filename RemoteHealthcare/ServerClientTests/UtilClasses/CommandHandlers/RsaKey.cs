using Newtonsoft.Json.Linq;
using Shared;
using Shared.Log;

namespace ServerClientTests.UtilClasses.CommandHandlers;

internal class RsaKey : ICommandHandler
{
    public void HandleCommand(DefaultClientConnection client, JObject ob)
    {
        var dict = new Dictionary<string, string>
        {
            {"\"_key_\"", Util.ByteArrayToString(client.GetRsaPublicKey())},
        };
        if(ob.ContainsKey("serial"))
            dict.Add("_serial_", ob["serial"]?.ToObject<string>() ??"_serial_");
        client.SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", dict, JsonFolder.Json.Path));
    }
    
}