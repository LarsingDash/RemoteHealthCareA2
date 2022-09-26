using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers.CommandHandlers;

public class ChangeData : ICommandHandler
{
    public void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["uuid"]?.ToObject<string>() != null)
        {

            JObject file = JsonFileReader.GetObject(ob["data"]!["uuid"]!.ToObject<string>()!,
                new Dictionary<string, string>(),
                JsonFolder.Data.Path + data.UserName + "\\");
            CheckValue(ob, file, "distance");
            CheckValue(ob, file, "speed");
            CheckValue(ob, file, "heartrate");
            JsonFileWriter.WriteObjectToFile(ob["data"]!["uuid"]!.ToObject<string>()!, file,
                JsonFolder.Data.Path + data.UserName + "\\");
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("ErrorResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "ok"},
                {"_id_", ob["id"]!.ToObject<string>()!}
            }, JsonFolder.ClientMessages.Path));
        }
        else
        {
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("ErrorResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "error"},
                {"_error_", "There is no uuid (session name)"},
                {"_id_", ob["id"]!.ToObject<string>()!}
            }, JsonFolder.ClientMessages.Path));
        }
        
    }

    private void CheckValue(JObject ob, JObject file, string value)
    {
        if (ob["data"]?[value]?.ToObject<JArray>() != null)
        {
            Console.WriteLine(file.ToString());
            Console.WriteLine(value);
            JArray distance = (JArray) file[value]!;
            distance.Merge((JArray) ob["data"]![value]!);
            file[value] = distance;

        }
    }
}