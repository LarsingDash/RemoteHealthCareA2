using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ClientApplication.ServerConnection.Communication.CommandHandlers;
using Newtonsoft.Json.Linq;
using NurseApplication.MVVM.ViewModel;

namespace NurseApplication.Communication.CommandHandlers
{
    public class Emergency : ICommandHandler
    {
        /// <summary>
        /// It sends the client the public RSA key of the server
        /// </summary>
        /// <param name="client">The client that sent the command</param>
        /// <param name="ob">The JSON object that was sent from the client.</param>
        public async void HandleCommand(Client client, JObject ob)
        {
            string bikeId = ob["data"]!["bikeId"]!.ToObject<string>()!;
            string username = ob["data"]!["username"]!.ToObject<string>()!;
            await Dispatcher.FromThread(App.GetThreadInstance())!.InvokeAsync((() =>
            {
                NurseViewModel.NurseModel.AddAlert(username, bikeId);
            }));
        }
    }
    
}