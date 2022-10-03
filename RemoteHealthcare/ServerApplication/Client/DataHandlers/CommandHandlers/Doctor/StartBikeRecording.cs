using System.Globalization;
using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers.CommandHandlers;

public class StartBikeRecording : ICommandHandler
{
    /// <summary>
    /// It creates a new file in the user's data folder, and writes a json object to it
    /// </summary>
    /// <param name="server">The server object</param>
    /// <param name="data">The client that sent the message</param>
    /// <param name="ob">The JObject that was sent from the client.</param>
    public void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["username"]?.ToObject<string>() == null)
        {
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecordingResponse",new Dictionary<string, string>()
                        {
                            {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                            {"_status_", "error"},
                            {"_error_", "There is no username"}
                        }, JsonFolder.ClientMessages.Path));
            return;
        }
        if (ob["data"]?["session-name"]?.ToObject<string>() != null)
        {
            var patient = server.GetUser(ob["data"]!["username"]!.ToObject<string>()!);
            if (patient == null)
            {
                data.SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecordingResponse",new Dictionary<string, string>()
                {
                    {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                    {"_status_", "error"},
                    {"_error_", "This username is not active"}
                }, JsonFolder.ClientMessages.Path));
                return;
            }

            if (patient.DataHandler.GetType() != typeof(ClientHandler))
            {
                data.SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecordingResponse",new Dictionary<string, string>()
                {
                    {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                    {"_status_", "error"},
                    {"_error_", "This user is not a patient"}
                }, JsonFolder.ClientMessages.Path));
                return;
            }
            
            string json = JsonFileReader.GetObjectAsString("BikeSessionFormat.json", new Dictionary<string, string>()
            {
                {"_sessionname_", ob["data"]!["session-name"]!.ToObject<string>()!},
                {"_starttime_", DateTime.Now.ToString(CultureInfo.InvariantCulture)}
            }, JsonFolder.Json.Path);
            string fileName = Util.RandomString();
            string exactFileName = fileName + ".txt";
            
            string totalPath = JsonFolder.Data.Path + patient.UserName + "\\" + exactFileName;
            JsonFileWriter.WriteTextToFileEncrypted(exactFileName, json, JsonFolder.Data.Path+data.UserName+"\\");
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecordingResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "ok"},
                {"_uuid_", fileName},
                {"_name_", patient.UserName}
            }, JsonFolder.ClientMessages.Path));
            patient.SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecordingResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "ok"},
                {"_uuid_", fileName}
            }, JsonFolder.ClientMessages.Path));
        }
        else
        {
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecordingResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "error"},
                {"_error_", "There is no session name"}
            }, JsonFolder.ClientMessages.Path));
        }
    }
}