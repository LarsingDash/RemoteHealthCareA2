using System.Globalization;
using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers.CommandHandlers;

public class StartBikeRecording : ICommandHandler
{
    public void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["session-name"]?.ToObject<string>() == null)
        {
            string json = JsonFileReader.GetObjectAsString("BikeSesionFormat.json", new Dictionary<string, string>()
            {
                {"_sessionname_", ob["data"]["session-name"].ToObject<string>()},
                {"_starttime_", DateTime.Now.ToString(CultureInfo.InvariantCulture)}
            });
            JsonFileWriter.WriteTextToFile(Util.RandomString(), json, JsonFolder.Data.Path+data.UserName+"\\");
        }
        else
        {
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecordingResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "error"},
                {"_error_", "There is no session name"}
            }));
        }
    }
}