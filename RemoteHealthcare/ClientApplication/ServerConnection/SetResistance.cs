using System;
using Newtonsoft.Json.Linq;
using Shared.Log;

namespace ClientApplication.ServerConnection;

public class SetResistance : ICommandHandler
{
    /// <summary>
    /// It takes the resistance value from the JSON object, converts it to an integer, and sets the resistance of the bike
    /// to that value
    /// </summary>
    /// <param name="Client">The client that sent the command</param>
    /// <param name="JObject">The JSON object that was sent from the client.</param>
    public void HandleCommand(Client client, JObject ob)
    {
        string? resistance = ob["data"]?["resistance"]?.ToObject<string>();
        App.GetBikeHandlerInstance().Bike.SetResistanceAsync(int.Parse(resistance!));
        Logger.LogMessage(LogImportance.Information, resistance);
    }
}