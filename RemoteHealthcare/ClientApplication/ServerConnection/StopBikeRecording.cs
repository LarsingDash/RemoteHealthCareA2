using Newtonsoft.Json.Linq;

namespace ClientApplication.ServerConnection;

public class StopBikeRecording : ICommandHandler
{
    public void HandleCommand(Client client, JObject ob)
    {
        client.SetCurrentBikeRecording("");
    }
}