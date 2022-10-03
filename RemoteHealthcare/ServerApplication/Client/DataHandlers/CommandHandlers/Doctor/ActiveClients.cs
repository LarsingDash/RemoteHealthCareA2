using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;
using Shared;
using Shared.Log;

namespace ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

public class ActiveClients : ICommandHandler
{
    public void HandleMessage(Server server, ClientData data, JObject ob)
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