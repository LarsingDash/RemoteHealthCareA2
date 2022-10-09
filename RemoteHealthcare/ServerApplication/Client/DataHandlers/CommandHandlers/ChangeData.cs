using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;
using Shared;

namespace ServerApplication.Client.DataHandlers.CommandHandlers;

public class ChangeData : CommandHandler
{
    /// <summary>
    /// It checks if the data is valid, if it is, it adds the data to the file
    /// </summary>
    /// <param name="server">The server that the message was sent to.</param>
    /// <param name="data">The ClientData object of the client that sent the message</param>
    /// <param name="ob">The JObject that was sent by the client.</param>
    public override void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["uuid"]?.ToObject<string>() != null)
        {
            string uuid = ob["data"]!["uuid"]!.ToObject<string>()!;
            string fileName = uuid + ".txt";
            
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
            if (server.SubscribedSessions.ContainsKey(uuid))
            {
                JObject message = JsonFileReader.GetObject("UpdateValues", new Dictionary<string, string>()
                {
                    {"uuid", uuid},
                }, JsonFolder.ClientMessages.Path);
                CheckValueInData(ob, message, "distance");
                CheckValueInData(ob, message, "speed");
                CheckValueInData(ob, message, "heartrate");
                foreach (var clientData in server.SubscribedSessions[uuid])
                {
                    clientData.SendEncryptedData(message.ToString());
                }
            }
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("ErrorResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "ok"},
                {"_id_", ob["id"]!.ToObject<string>()!}
            }, JsonFolder.ClientMessages.Path));
        }
        else
        {
            //Sending error message(no uuid)
            SendEncryptedError(data,ob,"There is no uuid(Session name");
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
            JArray distance = (JArray) file[value]!;
            distance.Merge((JArray) ob["data"]![value]!);
            file[value] = distance;
        }
    }
    
    /// <summary>
    /// If the value exists in the object, merge the arrays
    /// </summary>
    /// <param name="ob">The JObject that is being merged into the file.</param>
    /// <param name="file">The JObject that is being merged into the file.</param>
    /// <param name="value">The value to check for in the data object.</param>
    private void CheckValueInData(JObject ob, JObject file, string value)
    {
        if (ob["data"]?[value]?.ToObject<JArray>() != null)
        {
            JArray distance = (JArray) file["data"]![value]!;
            distance.Merge((JArray) ob["data"]![value]!);
            file["data"]![value] = distance;
        }
    }
}