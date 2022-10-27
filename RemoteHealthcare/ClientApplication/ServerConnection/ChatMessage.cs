using ClientApplication.Model;
using ClientApplication.ViewModel;
using ClientSide.VR2;
using Newtonsoft.Json.Linq;

namespace ClientApplication.ServerConnection;

public class ChatMessage : ICommandHandler
{
	/// <summary>
	/// > When the server sends a message, add it to the list of messages
	/// </summary>
	/// <param name="Client">The client that sent the command</param>
	/// <param name="JObject">The JSON object that was sent from the server.</param>
	public void HandleCommand(Client client, JObject ob)
    {
        App.CurrentDispatcher.Invoke(() =>
        {
            string? message = ob["data"]?["message"]?.ToObject<string>();
            string? sender = ob["data"]?["sender"]?.ToObject<string>();
            if (message != null)
            {
                DataViewModel.model.AddMessage(message);
                if (sender != null)
                {
                    App.GetVrClientInstance().PanelController?.UpdateChat(sender, message);
                }
            }
            
        });
        
    }
}