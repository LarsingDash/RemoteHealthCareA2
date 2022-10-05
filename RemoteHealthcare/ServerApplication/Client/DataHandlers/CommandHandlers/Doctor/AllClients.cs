
using Newtonsoft.Json.Linq;
using Shared.Log;
using Shared;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

public class AllClients : CommandHandler
{
    /// <summary>
    /// It gets all the directories in the data folder, converts them to strings, and sends them to the client
    /// </summary>
    /// <param name="server">The server instance.</param>
    /// <param name="data">The client that sent the message</param>
    /// <param name="ob">The JObject that was sent from the client.</param>
    public override void HandleMessage(Server server, ClientData data, JObject ob)
    {
        string[] dirs = Directory.GetDirectories(JsonFolder.Data.Path, "*", SearchOption.TopDirectoryOnly);
        string[] dirsName = Array.ConvertAll(dirs, s => "\"" + Path.GetFileName(s) + "\"")!;
        string dirsData = Util.ArrayToString(dirsName);
        data.SendEncryptedData(JsonFileReader.GetObjectAsString("AllClientsResponse", new Dictionary<string, string>()
        {
            { "_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_" },
            { "_status_", "ok"},
            { "\"_users_\"", dirsData}
        }, JsonFolder.ClientMessages.Path));
    }
}