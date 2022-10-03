using System.Globalization;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;
using Shared;

namespace ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

public class ChatMessage : ICommandHandler
{
    /// <summary>
    /// It sends a message to the receiver
    /// </summary>
    /// <param name="Server">The server instance</param>
    /// <param name="ClientData">The client that sent the message</param>
    /// <param name="JObject">The JObject that was sent from the client.</param>
    /// <returns>
    /// A string
    /// </returns>
    public void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["message"]?.ToObject<string>() == null)
        {
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("ChatMessageResponse",
                new Dictionary<string, string>()
                {
                    { "_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_" },
                    { "_status_", "error" },
                    { "_error_", "There is no message" }
                }, JsonFolder.ClientMessages.Path));
            return;
        }

        if (ob["data"]?["receiver"]?.ToObject<string>() == null)
        {
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("ChatMessageResponse",
                new Dictionary<string, string>()
                {
                    { "_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_" },
                    { "_status_", "error" },
                    { "_error_", "There is no receiver" }
                }, JsonFolder.ClientMessages.Path));
            return;
        }

        data.SendEncryptedData(JsonFileReader.GetObjectAsString("ForwardChatMessage", new Dictionary<string, string>()
        {
            { "_message_", ob["data"]!["message"]!.ToObject<string>()! },
            { "_sender_", data.UserName },
            { "_time_", DateTime.Now.ToString(CultureInfo.CurrentCulture) }
        }, JsonFolder.ClientMessages.Path));
    }
}
