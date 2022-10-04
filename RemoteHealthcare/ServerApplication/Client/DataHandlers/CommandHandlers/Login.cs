using Newtonsoft.Json.Linq;
using Shared.Log;
using Shared;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers.CommandHandlers
{
    public class Login : CommandHandler
    {
        /// <summary>
        /// It checks if the client is a doctor or a client, and if it's a client, it sets the data handler to a new
        /// ClientHandler
        /// </summary>
        /// <param name="server">The server that the message was sent to.</param>
        /// <param name="data">The ClientData object that is associated with the client that sent the message.</param>
        /// <param name="ob">The JObject that was received from the client.</param>
        public override void HandleMessage(Server server, ClientData data, JObject ob)
        {
            if (ob["data"]?["type"]?.ToObject<string>() == null)
            {
                //Sending error message(No type)
                SendEncryptedError(data,ob,"No type entered");
                return;
            }
            switch (ob["data"]!["type"]!.ToObject<string>())
            {
                case "Client":
                {
                    data.UserName = ob["data"]?["username"]?.ToObject<string>() ?? "Unknown";
                    data.DataHandler = new ClientHandler(data);
                    data.SendEncryptedData(JsonFileReader.GetObjectAsString("LoginResponse", new Dictionary<string, string>()
                    {
                        {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                        {"_status_", "ok"}
                    }, JsonFolder.ClientMessages.Path));
                    
                    var totalPath = JsonFolder.Data.Path + data.UserName + "\\";
                    if (!Directory.Exists(totalPath))
                    {
                        (new FileInfo(totalPath)).Directory!.Create();
                        Logger.LogMessage(LogImportance.Information, "New user, adding: " + totalPath);
                    }
                    else
                    {
                        Logger.LogMessage(LogImportance.Information, "User logged in: " + totalPath);
                    }
                    return;
                }
                case "Doctor":
                {
                    data.UserName = ob["data"]?["username"]?.ToObject<string>() ?? "Unknown";
                    JArray creds = (JArray) JsonFileReader.GetObject("AccountDoctor.json", new Dictionary<string, string>(), JsonFolder.Data.Path)["doctors"]!;
                    JToken? foundToken = creds.FirstOrDefault(value => value["username"]!.ToObject<string>()!.Equals(data.UserName));

                    if (foundToken != null)
                    {
                        JObject foundCred = (JObject)foundToken;
                        if (foundCred["password"]!.ToObject<string>()!
                            .Equals(ob["data"]?["password"]?.ToObject<string>() ?? "Unknown"))
                        {
                            //Sending error message
                            SendEncryptedError(data,ob,"_error_");
                            data.DataHandler = new DoctorHandler(data);
                        }
                        else
                        {
                            //Sending error message(Username/password not correct)
                            SendEncryptedError(data,ob, "Username and/or password not correct");
                        }
                        return;
                    }
                    else
                    {
                        //Sending error message(Username/password not correct)
                        SendEncryptedError(data, ob, "Username and/or password not correct");
                    }

                    //TODO Check login credentials
                    break;
                }
            }
            //Sending error message(Type not recognized 1)
            SendEncryptedError(data,ob,"Type not recognized. 1");
            return;
        }
    }
}