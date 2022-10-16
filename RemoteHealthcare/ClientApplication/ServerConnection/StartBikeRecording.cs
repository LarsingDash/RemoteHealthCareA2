using Newtonsoft.Json.Linq;

namespace ClientApplication.ServerConnection;

public class StartBikeRecording : ICommandHandler
{
    public void HandleCommand(Client client, JObject ob)
    {
        string? uuid = ob["data"]?["uuid"]?.ToObject<string>();
        if (uuid != null)
        {
            client.SetCurrentBikeRecording(uuid);
        }
    }
}