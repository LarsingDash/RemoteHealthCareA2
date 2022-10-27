using System.Windows.Threading;
using ClientApplication.ServerConnection;
using ClientApplication.ServerConnection.Communication.CommandHandlers;
using DoctorApplication.MVVM.Model;
using DoctorApplication.MVVM.ViewModel;
using Newtonsoft.Json.Linq;
using Shared.Log;

namespace DoctorApplication.Communication.CommandHandlers;

public class UserStateChange : ICommandHandler
{
    /// <summary>
    /// It handles the login and logout events of the users
    /// </summary>
    /// <param name="Client">The client that sent the command</param>
    /// <param name="JObject">The data that was sent from the server.</param>
    /// <returns>
    /// A list of all the users currently logged in.
    /// </returns>
    public async void HandleCommand(Client client, JObject ob)
    {
        string? username = ob["data"]?["username"]?.ToObject<string>();
        string? type = ob["data"]?["type"]?.ToObject<string>();
        if (username == null || type == null)
        {
            return;
        }

        if (type.Equals("login"))
        {
            await Dispatcher.FromThread(App.GetThreadInstance())!.InvokeAsync(() =>
            {
                UserDataModel model = new UserDataModel(username);
                MainViewModel.MainViewM.users.Add(model);
            });
        }
        else if(type.Equals("logout"))
        {
            UserDataModel? model = null;
            foreach (var user in MainViewModel.MainViewM.users)
            {
                if (user.UserName.Equals(username))
                {
                    model = user;
                    break;
                }
            }

            if (model == null)
                return;
            Logger.LogMessage(LogImportance.Information, "Removing user from list : " + model.UserName);
            await Dispatcher.FromThread(App.GetThreadInstance())!.InvokeAsync((() =>
            {
                MainViewModel.MainViewM.users.Remove(model);
            }));
        }
        
        
    }
}