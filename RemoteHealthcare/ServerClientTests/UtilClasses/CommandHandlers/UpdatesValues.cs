using Newtonsoft.Json.Linq;
using NUnit.Framework.Internal;
using Shared;
using Shared.Log;
using Logger = Shared.Log.Logger;

namespace ServerClientTests.UtilClasses.CommandHandlers;

public class UpdatesValues : ICommandHandler
{
    public static int received = 0;
    public void HandleCommand(DefaultClientConnection client, JObject ob)
    {
        received++;
    }
}