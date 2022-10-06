using System.Globalization;
using Newtonsoft.Json.Linq;
using Shared;
using JsonFolder = ServerApplication.UtilData.JsonFolder;

namespace ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

public class StopBikeRecording : CommandHandler
{
    /// <summary>
    /// It gets the current values of the file, adds the end time, and writes the updated values to the file
    /// </summary>
    /// <param name="server">The server object</param>
    /// <param name="data">The client that sent the message</param>
    /// <param name="ob">The JObject that was sent from the client.</param>
    public override void HandleMessage(Server server, ClientData data, JObject ob)
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
            SendEncryptedError(data,ob,"There is no session name");
           
        }
    }
}