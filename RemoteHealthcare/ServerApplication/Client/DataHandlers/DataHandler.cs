using ClientSide.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerSide.CommandHandlers;
using SharedProject;
using SharedProject.Log;

namespace ServerApplication.DataHandlers;

public abstract class DataHandler
{
    public ClientData ClientData;
    public Dictionary<string, ICommandHandler> CommandHandler;
    public DataHandler(ClientData clientData)
    {
        this.ClientData = clientData;
        this.CommandHandler = new();
    }
    
    public void HandleMessage(ClientData clientData, JObject json)
    {
        if (!json.ContainsKey("id"))
        {
            Logger.LogMessage(LogImportance.Warn, $"Got message with no id from {clientData.UserName}: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
            return;
        }
        if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
        {
            Logger.LogMessage(LogImportance.Information, $"Got message from {clientData.UserName}: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
        }

        if (json.ContainsKey("serial"))
        {
            var serial = json["serial"]!.ToObject<string>();
            if (clientData.SerialCallbacks.ContainsKey(serial!))
            {
                clientData.SerialCallbacks[serial!].Invoke(json);
                clientData.SerialCallbacks.Remove(serial!);
                return;
            }
        }

        if (CommandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
        {
            CommandHandler[json["id"]!.ToObject<string>()!].HandleMessage(clientData.Server, clientData, json);
        }
        else
        {
            Logger.LogMessage(LogImportance.Warn, $"Got message from {clientData.UserName} but no commandHandler found: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
        }
    }
}