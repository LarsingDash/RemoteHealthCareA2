using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ServerApplication;
using SharedProject;

namespace ServerSide.CommandHandlers;

public class RSAKey : ICommandHandler
{
    public void HandleMessage(Server server, ClientData data, JObject ob)
    {
        var dict = new Dictionary<string, string>
        {
            {"\"_key_\"", Util.ByteArrayToString(server.GetRsaPublicKey())},
        };
        if(ob.ContainsKey("serial"))
            dict.Add("_serial_", ob["serial"]?.ToObject<string>() ??"_serial_");
        data.SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", dict, JsonFolder.ClientMessages.path));
    }
}