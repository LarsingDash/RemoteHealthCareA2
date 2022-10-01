using System.Globalization;
using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers.CommandHandlers;

public class StopBikeRecording : ICommandHandler
{
    /// <summary>
    /// It gets the current values of the file, adds the end time, and writes the updated values to the file
    /// </summary>
    /// <param name="server">The server object</param>
    /// <param name="data">The client that sent the message</param>
    /// <param name="ob">The JObject that was sent from the client.</param>
    public void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["uuid"]?.ToObject<string>() != null)
        {
            string fileName = ob["data"]!["uuid"]!.ToObject<string>()! + ".txt";
            
            //Getting current values
            JObject file = JsonFileReader.GetEncryptedObject(fileName,
                new Dictionary<string, string>(),
                JsonFolder.Data.Path + data.UserName + "\\");

            //Adding end time
            file["end-time"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            
            
            //Writing updated values
            JsonFileWriter.WriteTextToFileEncrypted(fileName, file.ToString(), JsonFolder.Data.Path+data.UserName+"\\");
             //JsonFileWriter.WriteTextToFile(fileName, file.ToString(), JsonFolder.Data.Path+data.UserName+"\\"); Debugging to see data
            
            //Sending ok response
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("StopBikeRecordingResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "ok"},
            }, JsonFolder.ClientMessages.Path));
        }
        else
        {
            //Sending error response
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("StopBikeRecordingResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "error"},
                {"_error_", "There is no session name"}
            }, JsonFolder.ClientMessages.Path));
        }
    }
}