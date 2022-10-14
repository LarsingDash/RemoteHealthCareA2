using Newtonsoft.Json.Linq;
using Shared.Log;

namespace ClientSide.VR2.CommandHandler;

public class CreateTunnel : ICommandHandlerVR
{
    public void HandleCommand(VRClient client, JObject ob)
    {
        string tunnelId = ob["data"]?["id"]?.ToObject<string>() ?? "";
        Logger.LogMessage(LogImportance.Information, $"Tunnel Created, ID: {tunnelId}");
        client.TunnelStartup(tunnelId);
    }
}