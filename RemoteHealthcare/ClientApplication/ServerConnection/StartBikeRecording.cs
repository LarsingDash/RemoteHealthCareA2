using Newtonsoft.Json.Linq;

namespace ClientApplication.ServerConnection;

public class StartBikeRecording : ICommandHandler
{
    /// <summary>
    /// > The client sets the current bike recording to the one with the given UUID
    /// </summary>
    /// <param name="Client">The client that sent the command.</param>
    /// <param name="JObject">The JSON object that was sent from the client.</param>
    public void HandleCommand(Client client, JObject ob)
    {
        string? uuid = ob["data"]?["uuid"]?.ToObject<string>();
        if (uuid != null)
        {
            client.SetCurrentBikeRecording(uuid);
            App.GetBikeHandlerInstance().Bike.OnStateChange(true);
        }
    }
}