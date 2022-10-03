using Newtonsoft.Json.Linq;

namespace ClientSide.VR.CommandHandlers.TunnelMessages;

public class SetTimeScene : ICommandHandler
{
    /// <summary>
    /// > This function is called when the server sends a message to the client with the command "GetTime"
    /// </summary>
    /// <param name="VRClient">The client that sent the command.</param>
    /// <param name="JObject">The JSON object that was sent from the server.</param>
    public void handleCommand(VRClient client, JObject ob)
    {
        Console.WriteLine("GetTime: " + ob);
    }
}