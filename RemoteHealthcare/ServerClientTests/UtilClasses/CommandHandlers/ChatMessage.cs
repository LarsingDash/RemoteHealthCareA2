using Newtonsoft.Json.Linq;
using Shared;

namespace ServerClientTests.UtilClasses.CommandHandlers;

public class ChatMessage : ICommandHandler
{
    public static int received = 0;
    public void HandleCommand(DefaultClientConnection client, JObject ob)
    {
        received++;
    }
}