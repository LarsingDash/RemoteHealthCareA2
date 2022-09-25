using Newtonsoft.Json.Linq;

namespace ClientSide.VR.CommandHandlers;

public interface CommandHandler
{
    /// <summary>
    /// This function is called when a command is received from the server
    /// </summary>
    /// <param name="VRClient">The client that sent the command.</param>
    /// <param name="JObject">The JSON object that was sent from the client.</param>
    public void handleCommand(VRClient client, JObject ob);
}