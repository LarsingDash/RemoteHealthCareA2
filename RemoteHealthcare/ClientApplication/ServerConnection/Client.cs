using System.Collections.Generic;
using System.Threading;
using ClientApplication.ServerConnection.Bike;
using ClientApplication.Util;
using Newtonsoft.Json;
using Shared;
using Shared.Log;

namespace ClientApplication.ServerConnection;

public class Client : DefaultClientConnection
{
    private Dictionary<string, ICommandHandler> commandHandler = new();
    private string currentBikeRecording = "";
    public Client()
    {
        Logger.LogMessage(LogImportance.Information, "Connection with Server started");
        commandHandler.Add("public-rsa-key", new RsaKey());
        
        Init(Shared.ServerConnection.Hostname, Shared.ServerConnection.Port, (json, encrypted) =>
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
            commandHandler.Add("forward-set-resistance", new SetResistance());
            commandHandler.Add("forward-chat-message", new ChatMessage());
            Thread.Sleep(500);
            SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
            {
                {"_type_", "Client"},
                {"_username_", "TestUsername"},
                {"_serial_", "TestSerial"},
                {"_password_", "TestPassword"}
            }, JsonFolder.ServerConnection.Path)); 
        }

        BikeHandler handler = App.GetBikeHandlerInstance();
        handler.Subscribe(DataType.Distance, value =>
        {
            // SendEncryptedData();
        });
    }

    public void SetCurrentBikeRecording(string uuid)
    {
        currentBikeRecording = uuid;
    }
}