using Newtonsoft.Json.Linq;
using Shared;

namespace ServerClientTests.UtilClasses.CommandHandlers;

internal interface ICommandHandler
{
    public void HandleCommand(DefaultClientConnection client, JObject ob);
}