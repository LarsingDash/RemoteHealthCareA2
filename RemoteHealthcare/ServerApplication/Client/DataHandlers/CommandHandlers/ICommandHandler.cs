using Newtonsoft.Json.Linq;

namespace ServerApplication.Client.DataHandlers.CommandHandlers
{
    public interface ICommandHandler
    {
        /// <summary>
        /// This function is called when a message is received from a client
        /// </summary>
        /// <param name="Server">The server object that the message was sent to.</param>
        /// <param name="ClientData">This is the data of the client that sent the message.</param>
        /// <param name="JObject">The JSON object that was sent to the server.</param>
        public void HandleMessage(Server server, ClientData data, JObject ob);
    }
}