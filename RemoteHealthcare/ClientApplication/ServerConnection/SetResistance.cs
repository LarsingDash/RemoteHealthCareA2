using Newtonsoft.Json.Linq;

namespace ClientApplication.ServerConnection;

public class SetResistance : ICommandHandler
{
    public void HandleCommand(Client client, JObject ob)
    {
        string? resistance = ob["data"]?["resistance"]?.ToObject<string>();
        App.GetBikeHandlerInstance().Bike.SetResistanceAsync(int.Parse(resistance!));
    }
}