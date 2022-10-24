using System.Collections.Generic;
using System.Threading;
using ClientApplication.ServerConnection.Communication;
using ClientApplication.ServerConnection.Communication.CommandHandlers;
using DoctorApplication.Communication.CommandHandlers;
using Shared;
using Shared.Log;
using Formatting = Newtonsoft.Json.Formatting;

namespace DoctorApplication.Communication;

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
        
        Init(ServerConnection.Hostname, ServerConnection.Port, (json, encrypted) =>
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
        commandHandler.Add("user-state-changed", new UserStateChange());

        Thread.Sleep(500);
    }
}