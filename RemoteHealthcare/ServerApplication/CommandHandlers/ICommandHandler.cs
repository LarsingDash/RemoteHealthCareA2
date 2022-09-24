using Newtonsoft.Json.Linq;
using ServerApplication;

namespace ServerSide.CommandHandlers;

public interface ICommandHandler
{
    public void HandleMessage(Server server, ClientData data, JObject ob);
}