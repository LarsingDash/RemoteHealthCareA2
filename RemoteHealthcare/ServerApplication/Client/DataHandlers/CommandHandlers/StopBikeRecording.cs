using System.Globalization;
using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers.CommandHandlers;

public class StopBikeRecording : ICommandHandler
{
    public void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["uuid"]?.ToObject<string>() != null)
        {
            
            JObject file = JsonFileReader.GetObject(ob["data"]!["uuid"]!.ToObject<string>()!,
                new Dictionary<string, string>(),
                JsonFolder.Data.Path + data.UserName + "\\");

            file["end-time"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            
            
            string filename = Util.RandomString();
            JsonFileWriter.WriteObjectToFile(ob["data"]!["uuid"]!.ToObject<string>()!, file, JsonFolder.Data.Path+data.UserName+"\\");
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("StopBikeRecordingResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "ok"},
            }));
        }
        else
        {
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("StopBikeRecordingResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "error"},
                {"_error_", "There is no session name"}
            }));
        }
    }
}