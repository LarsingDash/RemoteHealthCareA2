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
        Console.WriteLine("Handle bike recording");
        if (ob["data"]?["session-name"]?.ToObject<string>() != null)
        {
            string json = JsonFileReader.GetObjectAsString("BikeSessionFormat.json", new Dictionary<string, string>()
            {
                {"_sessionname_", ob["data"]!["session-name"]!.ToObject<string>()!},
                {"_starttime_", DateTime.Now.ToString(CultureInfo.InvariantCulture)}
            }, JsonFolder.Json.Path);
            string fileName = Util.RandomString();
            string exactFileName = fileName + ".txt";

            string totalPath = JsonFolder.Data.Path + data.UserName + "\\" + exactFileName;
            JsonFileWriter.WriteTextToFileEncrypted(exactFileName, json, JsonFolder.Data.Path+data.UserName+"\\");
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecordingResponse",new Dictionary<string, string>()
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