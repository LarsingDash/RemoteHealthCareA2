using Newtonsoft.Json.Linq;

namespace ClientSide.VR2.CommandHandler;

public interface ICommandHandlerVR
{
    /// <summary>
    /// > This function is called when a command is received from the client
    /// </summary>
    /// <param name="Client">The client that sent the command</param>
    /// <param name="JObject">The JSON object that was sent to the server.</param>
    public void HandleCommand(VRClient client, JObject ob);
}