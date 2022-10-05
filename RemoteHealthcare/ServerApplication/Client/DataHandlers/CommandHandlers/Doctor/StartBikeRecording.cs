using System.Globalization;
using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;
using Shared;

namespace ServerApplication.Client.DataHandlers.CommandHandlers;

public class StartBikeRecording : CommandHandler
{
    /// <summary>
    /// It creates a new file in the user's data folder, and writes a json object to it
    /// </summary>
    /// <param name="server">The server object</param>
    /// <param name="data">The client that sent the message</param>
    /// <param name="ob">The JObject that was sent from the client.</param>
    public override void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["username"]?.ToObject<string>() == null)
        {
            //Sending error message(no Username)
            SendEncryptedError(data,ob,"There is no username");
            return;
        }
        if (ob["data"]?["session-name"]?.ToObject<string>() != null)
        {
            var patient = server.GetUser(ob["data"]!["username"]!.ToObject<string>()!);
            if (patient == null)
            {
                //Sending error message(Username not active)
                SendEncryptedError(data, ob, "This username is not active");
                return;
            }

            if (patient.DataHandler.GetType() != typeof(ClientHandler))
            {
                //Sending error message(Not a patient)
                SendEncryptedError(data,ob,"This user is not a patient");
                return;
            }
            
            string json = JsonFileReader.GetObjectAsString("BikeSessionFormat.json", new Dictionary<string, string>()
            {
                {"_sessionname_", ob["data"]!["session-name"]!.ToObject<string>()!},
                {"_starttime_", DateTime.Now.ToString(CultureInfo.InvariantCulture)}
            }, JsonFolder.Json.Path);
            string fileName = Util.RandomString();
            string exactFileName = fileName + ".txt";
            JsonFileWriter.WriteTextToFileEncrypted(exactFileName, json, JsonFolder.Data.Path+patient.UserName+"\\");
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecordingResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "ok"},
                {"_uuid_", fileName},
                {"_name_", patient.UserName}    
            }, JsonFolder.ClientMessages.Path));
            patient.SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecordingResponse",new Dictionary<string, string>()
            {
                {"_serial_", "_serial_"},
                {"_status_", "ok"},
                {"_uuid_", fileName}
            }, JsonFolder.ClientMessages.Path));
        }
        else
        {
            //Sending error message(no Session name)
            SendEncryptedError(data,ob,"There is no session name");
        }
    }
}