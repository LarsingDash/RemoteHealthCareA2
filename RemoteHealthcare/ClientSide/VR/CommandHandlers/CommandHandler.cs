using Newtonsoft.Json.Linq;

namespace ClientSide.VR.CommandHandlers;

public interface CommandHandler
{
    public void handleCommand(VRClient client, JObject ob);
}