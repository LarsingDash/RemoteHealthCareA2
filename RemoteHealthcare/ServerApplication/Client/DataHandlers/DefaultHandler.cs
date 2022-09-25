using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerApplication.Client.DataHandlers.CommandHandlers;
using ServerApplication.Log;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers;

public class DefaultHandler : DataHandler
{
    public DefaultHandler(ClientData clientData) : base(clientData)
    {
        CommandHandler = new()
        {
            {"public-rsa-key", new RsaKey()},
            {"encryptedMessage", new EncryptedMessage(clientData.Server.Rsa)},
            {"login", new Login()}
        };
    }
    /// <summary>
    /// If the message has a serial, invoke the callback, otherwise, if the message has an id, invoke the command handler,
    /// otherwise, send an error message.
    /// </summary>
    /// <param name="ClientData">The ClientData object that is associated with the client that sent the message.</param>
    /// <param name="JObject">The json object that was sent from the client.</param>
    /// <returns>
    /// A string
    /// </returns>
    public override void HandleMessage(ClientData clientData, JObject json)
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
            clientData.SendEncryptedData(JsonFileReader.GetObjectAsString("ErrorResponse", new Dictionary<string, string>()
            {
                {"_id_", json["id"]?.ToObject<string>() ?? "NoIdFound"},
                {"_serial_", json["serial"]?.ToObject<string>() ?? "_serial_"},
                {"_status_", "error"},
                {"_error_", "ID not recognized, has client logged in?"}
            }, JsonFolder.ClientMessages.Path));
        }
    }
}