using System.Globalization;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;

namespace ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

public class ChatMessage : ICommandHandler
{
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
