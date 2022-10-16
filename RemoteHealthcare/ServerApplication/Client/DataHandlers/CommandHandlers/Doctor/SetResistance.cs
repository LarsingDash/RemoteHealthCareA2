using System.Globalization;
using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;
using Shared;
using Shared.Log;

namespace ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

public class SetResistance : CommandHandler
{
    public override void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["resistance"]?.ToObject<string>() == null)
        {
            //Sending error message(no Resistance)
            SendEncryptedError(data,ob, "There is no resistance");
            return;
        }
        if (ob["data"]?["user"]?.ToObject<string>() == null)
        {
            //Sending error message(no User)
            SendEncryptedError(data,ob, "There is no User");
            return;
        }
        var user = server.GetUser(ob["data"]!["user"]!.ToObject<string>()!);
        
        if (user != null)
        {
            //Sending message to Receiver
            user.SendEncryptedData(JsonFileReader.GetObjectAsString("ForwardSetResistance",
                new Dictionary<string, string>()
                {
                    { "_resistance_", ob["data"]!["resistance"]!.ToObject<string>()! },
                }, JsonFolder.ClientMessages.Path));
            Logger.LogMessage(LogImportance.Fatal,  ob["data"]?["resistance"]?.ToObject<string>());
            
            //Sending ok status
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("ErrorResponse",
                new Dictionary<string, string>()
                {
                    { "_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_" },
                    { "_id_", ob["id"]?.ToObject<string>() ?? "_id_" },
                    { "_status_", "ok"}
                }, JsonFolder.ClientMessages.Path));
        }
        else
        {
            //Sending error message
            SendEncryptedError(data, ob, "There is a receiver name given, but it could not be found");
            return;
        }
        
        
    }
}