using Newtonsoft.Json.Linq;

namespace ServerApplication.Client.DataHandlers.CommandHandlers;

public interface ICommandHandler
{
    public void HandleMessage(Server server, ClientData data, JObject ob);
}