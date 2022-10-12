using Newtonsoft.Json.Linq;

namespace ClientSide;

public interface ICommandHandler
{
    /// <summary>
    /// > This function is called when a command is received from the client
    /// </summary>
    /// <param name="Client">The client that sent the command</param>
    /// <param name="JObject">The JSON object that was sent to the server.</param>
    public void HandleCommand(ClientV2 client, JObject ob);
}