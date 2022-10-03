using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerApplication.Client.DataHandlers.CommandHandlers;
using Shared.Log;

namespace ServerApplication.Client.DataHandlers
{
    public abstract class DataHandler
    {
        public ClientData ClientData;
        public Dictionary<string, ICommandHandler> CommandHandler;
    
        public DataHandler(ClientData clientData)
        {
            this.ClientData = clientData;
            this.CommandHandler = new Dictionary<string, ICommandHandler>();
        }
    
        /// <summary>
        /// It checks if the message has a serial, if it does it checks if the client has a callback for that serial, if it does
        /// it calls the callback and removes it from the list, if it doesn't it checks if the message has an id, if it does it
        /// checks if the server has a command handler for that id, if it does it calls the command handler, if it doesn't it
        /// logs a warning
        /// </summary>
        /// <param name="ClientData">The clientData object of the client that sent the message.</param>
        /// <param name="JObject">The message that was sent from the client.</param>
        /// <returns>
        /// The return value is a string.
        /// </returns>
        public virtual void HandleMessage(ClientData clientData, JObject json, bool encrypted = false)
        {
            string extraText = encrypted ? "Encrypted " : "";
            if (!json.ContainsKey("id"))
            {
                Logger.LogMessage(LogImportance.Warn, $"Got {extraText}message with no id from {clientData.UserName}: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
                return;
            }
            if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
            {
                Logger.LogMessage(LogImportance.Information, $"Got {extraText}message from {clientData.UserName}: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
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
                Logger.LogMessage(LogImportance.Warn, $"Got {extraText}message from {clientData.UserName} but no commandHandler found: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
            }
        }
    }
}