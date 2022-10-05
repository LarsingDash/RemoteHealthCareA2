using Newtonsoft.Json.Linq;
using Shared.Log;
using Shared;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

public class HistoricClientData : CommandHandler
{
    /// <summary>
    /// It gets the username from the message, checks if the user exists, and if so, it sends the user's historic data
    /// </summary>
    /// <param name="server">The server that the message was sent to.</param>
    /// <param name="data">The client that sent the message.</param>
    /// <param name="ob">The JObject that was sent from the client.</param>
    public override void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["username"]?.ToObject<string>() != null)
        {
            string userName = ob["data"]!["username"]!.ToObject<string>()!;
            if (Directory.Exists(JsonFolder.Data.Path + userName))
            {
                JArray sendFile = new JArray();
                string[] files = Directory.GetFiles(JsonFolder.Data.Path + userName, "*.txt");
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    Console.WriteLine("path: " + file.Remove(file.Length - fileName.Length) + fileName);
                    Console.WriteLine("File: " + JsonFileReader.GetEncryptedText(fileName, new Dictionary<string, string>(), file.Remove(file.Length - fileName.Length)));
                    sendFile.Add(JObject.Parse(JsonFileReader.GetEncryptedText(fileName, new Dictionary<string, string>(), file.Remove(file.Length - fileName.Length))));
                }
                data.SendEncryptedData(JsonFileReader.GetObjectAsString("HistoricClientDataResponse", new Dictionary<string, string>()
                {
                    {"_user_", userName},
                    {"\"_session_\"", sendFile.ToString()} 
                }, JsonFolder.ClientMessages.Path));
            }
            else
            {
                //Sending error message(User not found)
                SendEncryptedError(data,ob,"User not found");
            }
        }
        else
        {
            //Sending error message(no username given)
            SendEncryptedError(data,ob,"No username given");
        }
    }
}