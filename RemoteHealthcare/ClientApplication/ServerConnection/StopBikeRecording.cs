using Newtonsoft.Json.Linq;

namespace ClientApplication.ServerConnection;

public class StopBikeRecording : ICommandHandler
{
    /// <summary>
    /// > This function is called when the client sends a message to the server with the command "SetCurrentBikeRecording"
    /// and the server will then set the current bike recording to an empty string
    /// </summary>
    /// <param name="Client">The client that sent the command</param>
    /// <param name="JObject">The JSON object that was sent to the server.</param>
    public void HandleCommand(Client client, JObject ob)
    {
        client.SetCurrentBikeRecording("");
    }
}