using Newtonsoft.Json.Linq;

namespace ServerApplication.Client.DataHandlers.CommandHandlers;

public class AlreadyLoggedIn : CommandHandler
{
    public override void HandleMessage(Server server, ClientData data, JObject ob)
    {
        SendEncryptedError(data, ob, "Already Logged In");
    }
}