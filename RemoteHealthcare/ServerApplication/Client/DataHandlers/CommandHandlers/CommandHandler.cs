using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;
using Shared;

namespace ServerApplication.Client.DataHandlers.CommandHandlers
{
    public abstract class CommandHandler
    {
        /// <summary>
        /// This function is called when a message is received from a client
        /// </summary>
        /// <param name="server">The server object that the message was sent to.</param>
        /// <param name="data">This is the data of the client that sent the message.</param>
        /// <param name="ob">The JSON object that was sent to the server.</param>
        public abstract void HandleMessage(Server server, ClientData data, JObject ob);

        public void SendEncryptedError(ClientData data, JObject ob , string errorMessage)
        {
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("ErrorResponse", new Dictionary<string, string>()
            {
                { "_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_" },
                { "_id_", ob["id"]?.ToObject<string>() ?? "_id_" },
                { "_status_", "error" },
                { "_error_", errorMessage }
            }, JsonFolder.ClientMessages.Path));
        }
    }
}