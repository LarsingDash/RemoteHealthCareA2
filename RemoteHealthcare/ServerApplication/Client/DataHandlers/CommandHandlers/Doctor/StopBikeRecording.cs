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
        if (ob["data"]?["username"]?.ToObject<string>() == null)
        {
            SendEncryptedError(data,ob,"No Username given");
            return;
        }
        string name = ob["data"]!["username"]!.ToObject<string>()!;
        ClientData? user = server.GetUser(name);
        if (user == null)
        {
            SendEncryptedError(data,ob,"Username given, but not found");
            return;
        }

        if (user.DataHandler.GetType() != typeof(ClientHandler))
        {
            SendEncryptedError(data,ob,"User is not a patient");
            return;
        }
        if (ob["data"]?["uuid"]?.ToObject<string>() != null)
        {
            if (!server.ActiveSessions.Contains(ob["data"]!["uuid"]!.ToObject<string>()!))
            {
                SendEncryptedError(data,ob, "Session is not found or active");
                return;
            }
            string fileName = ob["data"]!["uuid"]!.ToObject<string>()! + ".txt";
            
            //Getting current values
            JObject file = JsonFileReader.GetEncryptedObject(fileName,
                new Dictionary<string, string>(),
                JsonFolder.Data.Path + user.UserName + "\\");

            //Adding end time
            file["end-time"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            
            
            //Writing updated values
            JsonFileWriter.WriteTextToFileEncrypted(fileName, file.ToString(), JsonFolder.Data.Path+user.UserName+"\\");
             //JsonFileWriter.WriteTextToFile(fileName, file.ToString(), JsonFolder.Data.Path+data.UserName+"\\"); Debugging to see data
            
            //Sending ok response
            if (ob["data"]?["emergency-stop"]?.ToObject<string>() != null && ob["data"]!["emergency-stop"]!.ToObject<string>()!.Equals("emergencyStop"))
            {
                foreach (var cD in server.users)
                {
                    if (cD.DataHandler.GetType() == typeof(NurseHandler))
                    {
                        data.SendEncryptedData(JsonFileReader.GetObjectAsString("EmergencyResponse",new Dictionary<string, string>()
                        {
                            {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                            {"_status_", "ok"},
                        }, JsonFolder.ClientMessages.Path));
                        server.ActiveSessions.Remove(ob["data"]!["uuid"]!.ToObject<string>()!);
                    }
                }
            }
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("StopBikeRecordingResponse",new Dictionary<string, string>()
            {
                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "ok"},
            }, JsonFolder.ClientMessages.Path));
            server.ActiveSessions.Remove(ob["data"]!["uuid"]!.ToObject<string>()!);
        }
        else
        {
            //Sending error response 
            SendEncryptedError(data,ob,"There is no session name");
           
        }
    }
}