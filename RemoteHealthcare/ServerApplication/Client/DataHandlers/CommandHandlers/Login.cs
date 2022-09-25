using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers.CommandHandlers
{
    public class Login : ICommandHandler
    {
        /// <summary>
        /// It checks if the client is a doctor or a client, and if it's a client, it sets the data handler to a new
        /// ClientHandler
        /// </summary>
        /// <param name="server">The server that the message was sent to.</param>
        /// <param name="data">The ClientData object that is associated with the client that sent the message.</param>
        /// <param name="ob">The JObject that was received from the client.</param>
        public void HandleMessage(Server server, ClientData data, JObject ob)
        {
            if (ob["data"]?["type"]?.ToObject<string>() == null)
            {
                data.SendEncryptedData(JsonFileReader.GetObjectAsString("LoginResponse", new Dictionary<string, string>()
                {
                    {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                    {"_status_", "error"},
                    {"_error_", "No type entered"}
                }, JsonFolder.ClientMessages.Path));
                return;
            }
            switch (ob["data"]!["type"]!.ToObject<string>())
            {
                case "Client":
                {
                    data.UserName = ob["data"]?["username"]?.ToObject<string>() ?? "Unknown";
                    data.DataHandler = new ClientHandler(data);
                    data.SendEncryptedData(JsonFileReader.GetObjectAsString("LoginResponse", new Dictionary<string, string>()
                    {
                        {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                        {"_status_", "ok"}
                    }, JsonFolder.ClientMessages.Path));
                    return;
                }
                case "Doctor":
                {
                    //TODO Check login credentials
                    break;
                }
            }
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("LoginResponse", new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "error"},
                {"_error_", "Type not recognized."}
            }, JsonFolder.ClientMessages.Path));
            return;
        }
    }
}