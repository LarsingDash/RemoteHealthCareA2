using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ServerApplication;

namespace DoctorApplication.Communication.CommandHandlers
{
    public class RsaKey : ICommandHandler
    {
        public void HandleCommand(Client client, JObject ob)
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
}