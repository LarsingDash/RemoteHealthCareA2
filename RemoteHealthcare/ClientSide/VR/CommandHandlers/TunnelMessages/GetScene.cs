using Newtonsoft.Json.Linq;

namespace ClientSide.VR.CommandHandlers.TunnelMessages;

public class GetScene : ICommandHandler
{
    /// <summary>
    /// > This function is called when the client receives a command from the server
    /// </summary>
    /// <param name="VRClient">The client that sent the command</param>
    /// <param name="JObject">The JSON object that was sent from the client.</param>
    public void handleCommand(VRClient client, JObject ob)
    {
        client.tunnel.UpdateValue(TunnelDataType.Scene, ob);
    }
}