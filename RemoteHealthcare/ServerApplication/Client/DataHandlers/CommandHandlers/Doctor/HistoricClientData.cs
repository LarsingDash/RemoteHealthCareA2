using Newtonsoft.Json.Linq;
using Shared.Log;
using Shared;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

public class HistoricClientData : ICommandHandler
{
    public void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["username"]?.ToObject<string>() != null)
        {
            string userName = ob["data"]!["username"]!.ToObject<string>()!;
            if (Directory.Exists(JsonFolder.Data.Path + userName))
            {
                JArray sendFile = new JArray();
                string[] files = Directory.GetFiles(JsonFolder.Data.Path + userName, "*.txt");
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    Console.WriteLine("path: " + file.Remove(file.Length - fileName.Length) + fileName);
                    Console.WriteLine("File: " + JsonFileReader.GetEncryptedText(fileName, new Dictionary<string, string>(), file.Remove(file.Length - fileName.Length)));
                    sendFile.Add(JObject.Parse(JsonFileReader.GetEncryptedText(fileName, new Dictionary<string, string>(), file.Remove(file.Length - fileName.Length))));
                }
                data.SendEncryptedData(JsonFileReader.GetObjectAsString("HistoricClientDataResponse", new Dictionary<string, string>()
                {
                    {"_user_", userName},
                    {"\"_session_\"", sendFile.ToString()} 
                }, JsonFolder.ClientMessages.Path));
            }
            else
            {
                data.SendEncryptedData(JsonFileReader.GetObjectAsString("ErrorResponse",new Dictionary<string, string>()
                {
                    {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                    {"_status_", "error"},
                    {"_error_", "User not found."},
                    {"_id_", ob["id"]!.ToObject<string>()!}
                }, JsonFolder.ClientMessages.Path));
            }
        }
        else
        {
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("ErrorResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "error"},
                {"_error_", "No username given."},
                {"_id_", ob["id"]!.ToObject<string>()!}
            }, JsonFolder.ClientMessages.Path));
        }
    }
}