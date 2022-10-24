#nullable enable
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
                    string? checkUserName = ob["data"]?["username"]?.ToObject<string>();
                    if (checkUserName == null)
                    {
                        SendEncryptedError(data,ob, "No Username entered");
                        return;
                    }

                    ClientData? user = server.GetUser(checkUserName!);
                    if (user != null)
                    {
                        SendEncryptedError(data, ob, "Username is already logged in.");
                        return;
                    }
                    JArray creds = (JArray) JsonFileReader.GetObject("AccountClient.json", new Dictionary<string, string>(), JsonFolder.Data.Path)["clients"]!;
                    JToken? foundToken = creds.FirstOrDefault(value => value["username"]!.ToObject<string>()!.Equals(checkUserName), null);

                    if (foundToken != null)
                    {
                        JObject foundCred = (JObject)foundToken;
                        if (foundCred["password"]!.ToObject<string>()!
                            .Equals(ob["data"]?["password"]?.ToObject<string>() ?? "Unknown"))
                        {
                            data.SendEncryptedData(JsonFileReader.GetObjectAsString("LoginResponse", new Dictionary<string, string>()
                            {
                                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                                {"_status_", "ok"},
                                {"_error_", "_error_"}
                            }, JsonFolder.ClientMessages.Path));
                            data.UserName = ob["data"]?["username"]?.ToObject<string>() ?? "Unknown";
                            data.AddInfo("bikeId", ob["data"]?["bikeId"]?.ToObject<string>() != null ? ob["data"]!["bikeId"]!.ToObject<string>()! : "Not Found");
                            data.DataHandler = new ClientHandler(data);
                            PatientLoggedIn(server, checkUserName);
                        }
                        else
                        {
                            //Sending error message(Username/password not correct)
                            SendEncryptedError(data,ob, "Username and/or password not correct");
                        }
                        return;
                    }

                    var totalPath = JsonFolder.Data.Path + checkUserName + "\\";
                    if (!Directory.Exists(totalPath))
                    {
                        (new FileInfo(totalPath)).Directory!.Create();
                        Logger.LogMessage(LogImportance.Information, "New user, adding: " + totalPath);
                    }
                    else
                    {
                        Logger.LogMessage(LogImportance.Information, "User logged in: " + totalPath);
                    }

                    JObject newUser = new JObject();
                    newUser.Add("username", checkUserName);
                    newUser.Add("password", ob["data"]?["password"]?.ToObject<string>() ?? "Unknown");
                    creds.Add(newUser);

                    JObject finalObject = new JObject();
                    finalObject.Add("clients", creds);
                    
                    JsonFileWriter.WriteTextToFile("AccountClient.json", finalObject.ToString(), JsonFolder.Data.Path);
                    data.UserName = ob["data"]?["username"]?.ToObject<string>() ?? "Unknown";
                    data.AddInfo("bikeId", ob["data"]?["bikeId"]?.ToObject<string>() != null ? ob["data"]!["bikeId"]!.ToObject<string>()! : "Not Found");
                    data.DataHandler = new ClientHandler(data);
                    data.SendEncryptedData(JsonFileReader.GetObjectAsString("LoginResponse", new Dictionary<string, string>()
                    {
                        {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                        {"_status_", "ok"},
                        {"_error_", "_error_"}
                    }, JsonFolder.ClientMessages.Path));
                    PatientLoggedIn(server, checkUserName);
                    return;
                }
                case "Doctor":
                {
                    data.UserName = ob["data"]?["username"]?.ToObject<string>() ?? "Unknown";
                    JArray creds = (JArray) JsonFileReader.GetObject("AccountDoctor.json", new Dictionary<string, string>(), JsonFolder.Data.Path)["doctors"]!;
                    JToken? foundToken = creds.FirstOrDefault(value => value["username"]!.ToObject<string>()!.Equals(data.UserName), null);

                    if (foundToken != null)
                    {
                        JObject foundCred = (JObject)foundToken;
                        if (foundCred["password"]!.ToObject<string>()!
                            .Equals(ob["data"]?["password"]?.ToObject<string>() ?? "Unknown"))
                        {
                            data.SendEncryptedData(JsonFileReader.GetObjectAsString("LoginResponse", new Dictionary<string, string>()
                            {
                                {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                                {"_status_", "ok"},
                                {"_error_", "_error_"}
                            }, JsonFolder.ClientMessages.Path));
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
                case "Nurse":
                {
                    data.SendEncryptedData(JsonFileReader.GetObjectAsString("LoginResponse", new Dictionary<string, string>()
                    {
                        {"_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_"},
                        {"_status_", "ok"},
                        {"_error_", "_error_"}
                    }, JsonFolder.ClientMessages.Path));
                    data.DataHandler = new NurseHandler(data);
                    return;
                }
            }
            //Sending error message(Type not recognized 1)
            SendEncryptedError(data,ob,"Type not recognized.");
            return;
        }

        public void PatientLoggedIn(Server server,string username)
        {
            string text = JsonFileReader.GetObjectAsString("UserStateChange", new Dictionary<string, string>()
            {
                {"_username_", username},
                {"_type_", "login"}
            }, JsonFolder.ClientMessages.Path);
            foreach (var client in server.users)
            {
                if (client.DataHandler.GetType() == typeof(DoctorHandler))
                {
                    client.SendEncryptedData(text);
                }
            }
        }
    }
}