using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;
using Shared;
using Shared.Log;

namespace ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

public class HistoricClientDataSession : CommandHandler
{
    public override void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["name"]?.ToObject<string>() == null)
        {
            SendEncryptedError(data, ob, "No Session name");
            return;
        }
        if (ob["data"]?["username"]?.ToObject<string>() != null)
        {
            string userName = ob["data"]!["username"]!.ToObject<string>()!;
            string name = ob["data"]!["name"]!.ToObject<string>()!;
            if (!Directory.Exists(JsonFolder.Data.Path + userName))
            {
                SendEncryptedError(data, ob, "Session not found (directory not found)");
                return;
            }
            if (File.Exists(JsonFolder.Data.Path + userName + "\\" + name + ".txt"))
            {
                JObject ses = JObject.Parse(JsonFileReader.GetEncryptedText(name + ".txt",
                    new Dictionary<string, string>(), JsonFolder.Data.Path + userName + "\\"));
                data.SendEncryptedData(JsonFileReader.GetObjectAsString("HistoricClientDataSessionResponse", new Dictionary<string, string>()
                {
                    {"_name_", userName},
                    {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                    {"\"_session_\"", ses.ToString()}, 
                    {"_status_", "ok"}
                }, JsonFolder.ClientMessages.Path));
            }
            else
            {
                //Sending error message(User not found)
                Logger.LogMessage(LogImportance.Warn, "Session not found: " + JsonFolder.Data.Path + userName + "\\" + name + ".txt");
                SendEncryptedError(data,ob,"Session not found (session not found in directory)");
            }
        }
        else
        {
            //Sending error message(no username given)
            SendEncryptedError(data,ob,"No username given");
        }
    }
}