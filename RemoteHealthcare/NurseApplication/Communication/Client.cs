using System.Collections.Generic;
using System.Threading;
using ClientApplication.ServerConnection.Communication;
using ClientApplication.ServerConnection.Communication.CommandHandlers;
using Newtonsoft.Json;
using Shared;
using Shared.Log;

namespace NurseApplication.Communication;

public class Client : DefaultClientConnection
{
    
    private Dictionary<string, ICommandHandler> commandHandler = new();
    public List<string> hideMessages = new List<string>();
    
    public Client()
    {
        commandHandler.Add("public-rsa-key", new RsaKey());
        hideMessages = new List<string>()
        {
        };
        
        Init("127.0.0.1", 2460, (json, encrypted) =>
        {
            string extraText = encrypted ? "Encrypted " : "";
            if (commandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
            {
                if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage") && !hideMessages.Contains(json["id"]!.ToObject<string>()!))
                {
                    Logger.LogMessage(LogImportance.Information, $"Got {extraText}message from server: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
                }
                commandHandler[json["id"]!.ToObject<string>()!].HandleCommand(this, json);
            }
            else
            {
                Logger.LogMessage(LogImportance.Warn, $"Got {extraText}message from server but no commandHandler found: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
            }
        });
        
        commandHandler.Add("encryptedMessage", new EncryptedMessage(Rsa));

        Thread.Sleep(500);
        var serial = Util.RandomString();
        SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
        {
            {"_serial_", serial}
        }, JsonFolder.Json.Path));

        AddSerialCallbackTimeout(serial, ob =>
        {
            if (!ob["data"]!["status"]!.ToObject<string>()!.Equals("ok"))
            {
                Logger.LogMessage(LogImportance.Fatal, "Could not login as Nurse (status not ok) ErrorMessage: " + ob["data"]!["error"]!.ToObject<string>());
            }
        }, () =>
        {
            Logger.LogMessage(LogImportance.Fatal, "Could not login as Nurse. No response from server");
        }, 1000);
    }
}