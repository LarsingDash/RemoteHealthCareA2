using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;
using Shared;

namespace ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

public class SubscribeToSession : CommandHandler
{
    public SubscribeToSession()
    {
        
    }

    public override void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["uuid"]?.ToObject<string>() == null)
        {
            SendEncryptedError(data, ob, "No uuid found");
            return;
        }
        if (!server.ActiveSessions.Contains(ob["data"]!["uuid"]!.ToObject<string>()!))
        {
            SendEncryptedError(data, ob, "Session is not active");
            return;
        }
        string uuid = ob["data"]!["uuid"]!.ToObject<string>()!;
        if (server.SubscribedSessions.ContainsKey(uuid))
        {
            List<ClientData> list = server.SubscribedSessions[uuid];
            list.Add(data);
            server.SubscribedSessions.Remove(uuid);
            server.SubscribedSessions.Add(uuid, list);
        }
        else
        {
            server.SubscribedSessions.Add(uuid, new List<ClientData>() {data});
        }
        
        data.SendEncryptedData(JsonFileReader.GetObjectAsString("ErrorResponse", new Dictionary<string, string>()
        {
            { "_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_" },
            { "_status_", "ok"},
            {"_id_", ob["id"]!.ToObject<string>()!}
        }, JsonFolder.ClientMessages.Path));
        
    }
}