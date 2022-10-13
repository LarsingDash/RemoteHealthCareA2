using System.Collections.Generic;
using System.Threading;
using ClientApplication.Util;
using Newtonsoft.Json;
using Shared;
using Shared.Log;

namespace ClientApplication.ServerConnection;

public class Client : DefaultClientConnection
{
    private Dictionary<string, ICommandHandler> commandHandler = new();
    
    public Client()
    {
        commandHandler.Add("public-rsa-key", new RsaKey());
        
        Init("127.0.0.1", 2460, (json, encrypted) =>
        {
            string extraText = encrypted ? "Encrypted " : "";
            if (commandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
            {
                if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
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
        if (Connected)
        {
            commandHandler.Add("encryptedMessage", new EncryptedMessage(Rsa));
            Thread.Sleep(500);
            SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
            {
                {"_type_", "Client"},
                {"_username_", "TestUsername"},
                {"_serial_", "TestSerial"},
                {"_password_", "TestPassword"}
            }, JsonFolder.ServerConnection.Path)); 
        }
    }
}