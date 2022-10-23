using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NurseApplication.Communication;
using Shared;

namespace ClientApplication.ServerConnection.Communication.CommandHandlers
{
    public class Emergency : ICommandHandler
    {
        /// <summary>
        /// It sends the client the public RSA key of the server
        /// </summary>
        /// <param name="Client">The client that sent the command</param>
        /// <param name="JObject">The JSON object that was sent from the client.</param>
        public void HandleCommand(Client client, JObject ob)
        {
            string bikeId = ob["data"]!["bikeId"]!.ToObject<string>()!;
            string username = ob["data"]!["username"]!.ToObject<string>()!;
        }
    }
}