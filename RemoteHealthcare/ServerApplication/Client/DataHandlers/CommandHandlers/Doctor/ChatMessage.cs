using System.Globalization;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using ServerApplication.UtilData;
using Shared;

namespace ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

public class ChatMessage : CommandHandler
{
    /// <summary>
    /// It sends a message to the receiver
    /// </summary>
    /// <param name="server">The server instance</param>
    /// <param name="data">The client that sent the message</param>
    /// <param name="ob">The JObject that was sent from the client.</param>
    /// <returns>
    /// A string
    /// </returns>
    public override void HandleMessage(Server server, ClientData data, JObject ob)
    {
        if (ob["data"]?["message"]?.ToObject<string>() == null)
        {
            //Sending error message(no Message)
            SendEncryptedError(data,ob, "There is no message");
            return;
        }

        if (ob["data"]?["receiver"]?.ToObject<string>() == null)
        {
            //Sending error message(no Receiver)
            SendEncryptedError(data, ob, "There is no receiver");
            return;
        }

        var receiver = server.GetUser(ob["data"]!["receiver"]!.ToObject<string>()!);

        if (receiver != null)
        {
            //Sending message to Receiver
            receiver.SendEncryptedData(JsonFileReader.GetObjectAsString("ForwardChatMessage",
                new Dictionary<string, string>()
                {
                    { "_message_", ob["data"]!["message"]!.ToObject<string>()! },
                    { "_sender_", data.UserName},
                    { "_time_", DateTime.Now.ToString(CultureInfo.InvariantCulture) }
                }, JsonFolder.ClientMessages.Path));
            
            //Sending ok status
            data.SendEncryptedData(JsonFileReader.GetObjectAsString("ChatMessageResponse",
                new Dictionary<string, string>()
                {
                    { "_serial_", ob["serial"]?.ToObject<string>() ?? "_serial_" },
                    { "_status_", "ok"}
                }, JsonFolder.ClientMessages.Path));
        }
        else
        {
            //Sending error message
            SendEncryptedError(data, ob, "There is a receiver name given, but it could not be found");
            return;
        }

    }
}
