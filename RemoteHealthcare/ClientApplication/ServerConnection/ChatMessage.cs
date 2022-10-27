using ClientApplication.Model;
using ClientApplication.ViewModel;
using ClientSide.VR2;
using Newtonsoft.Json.Linq;

namespace ClientApplication.ServerConnection;

public class ChatMessage : ICommandHandler
{
	public void HandleCommand(Client client, JObject ob)
    {
        App.CurrentDispatcher.Invoke(() =>
        {
            string? message = ob["data"]?["message"]?.ToObject<string>();
            if (message != null)
            {
                DataViewModel.model.AddMessage(message);
                //data.AddMessage(message);
            }
        });
        
    }
}