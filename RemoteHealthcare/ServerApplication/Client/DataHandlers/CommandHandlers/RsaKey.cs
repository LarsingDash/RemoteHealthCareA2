using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;
using Shared;
using JsonFolder = ServerApplication.UtilData.JsonFolder;

namespace ServerApplication.Client.DataHandlers.CommandHandlers
{
    public class RsaKey : ICommandHandler
    {
        /// <summary>
        /// It sends the public RSA key to the client
        /// </summary>
        /// <param name="server">The server instance</param>
        /// <param name="data">The client that sent the message</param>
        /// <param name="ob">The Json object that was sent from the client.</param>
        public void HandleMessage(Server server, ClientData data, JObject ob)
        {
            var dict = new Dictionary<string, string>
            {
                {"\"_key_\"", Util.ByteArrayToString(server.GetRsaPublicKey())},
            };
            if(ob.ContainsKey("serial"))
                dict.Add("_serial_", ob["serial"]?.ToObject<string>() ??"_serial_");
            data.SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", dict, JsonFolder.ClientMessages.Path));
        }
        
    }
}