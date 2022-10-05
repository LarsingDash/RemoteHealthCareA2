using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers.CommandHandlers;

public class ChangeData : ICommandHandler
{
    /// <summary>
    /// It checks if the data is valid, if it is, it adds the data to the file
    /// </summary>
    /// <param name="Server">The server that the message was sent to.</param>
    /// <param name="ClientData">The ClientData object of the client that sent the message</param>
    /// <param name="JObject">The JObject that was sent by the client.</param>
    public void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["uuid"]?.ToObject<string>() != null)
        {
            string fileName = ob["data"]!["uuid"]!.ToObject<string>()! + ".txt";
            
            //Getting current Values
            JObject file = JsonFileReader.GetEncryptedObject(fileName,
                new Dictionary<string, string>(),
                JsonFolder.Data.Path + data.UserName + "\\");
            
            //Adding new Values
            CheckValue(ob, file, "distance");
            CheckValue(ob, file, "speed");
            CheckValue(ob, file, "heartrate");
            
            //Writing combined Values
            JsonFileWriter.WriteTextToFileEncrypted(fileName, file.ToString(),
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

    /// <summary>
    /// If the value exists in the object, merge the arrays and add it to the file
    /// </summary>
    /// <param name="JObject">The object that is being checked for the value</param>
    /// <param name="JObject">The object that is being checked for the value</param>
    /// <param name="value">the name of the value you want to merge</param>
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