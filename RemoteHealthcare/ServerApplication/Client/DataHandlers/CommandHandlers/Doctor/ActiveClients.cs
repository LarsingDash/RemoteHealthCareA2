using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;
using Shared;
using Shared.Log;

namespace ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

public class ActiveClients : CommandHandler
{
    /// <summary>
    /// It sends a list of all the users that are currently connected to the server
    /// </summary>
    /// <param name="Server">The server instance</param>
    /// <param name="ClientData">The ClientData object of the client that sent the message.</param>
    /// <param name="JObject">The JObject that was sent to the server.</param>
    public override void HandleMessage(Server server, ClientData data, JObject ob)
    {
        data.SendEncryptedData(JsonFileReader.GetObjectAsString("ActiveClientsResponse", new Dictionary<string,string>()
        {
            {"\"_users_\"", Util.ArrayToString((
                from u
                    in server.users
                where u.DataHandler is ClientHandler
                select u.UserName).ToArray(),true, true)},
            {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
            {"_status_", "ok"},
        }, JsonFolder.ClientMessages.Path));
    }
}