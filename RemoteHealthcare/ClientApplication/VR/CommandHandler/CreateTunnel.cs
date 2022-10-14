using FontAwesome.Sharp;
using Newtonsoft.Json.Linq;
using Shared.Log;

namespace ClientSide.VR2.CommandHandler;

public class CreateTunnel : ICommandHandlerVR
{
    public void HandleCommand(VRClient client, JObject ob)
    {
        string tunnelId = ob["data"]?["id"]?.ToObject<string>() ?? "";
        if (tunnelId.Length == 0)
        {
            Logger.LogMessage(LogImportance.Error, "Could not create tunnel: " + ob["data"]!["msg"]!.ToObject<string>());
        }
        else
        {
            Logger.LogMessage(LogImportance.Information, $"Tunnel Created, ID: {tunnelId}");
            client.TunnelStartup(tunnelId);
        }
    }
}