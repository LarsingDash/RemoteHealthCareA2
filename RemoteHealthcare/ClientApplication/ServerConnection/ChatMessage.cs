using ClientApplication.Model;
using ClientSide.VR2;
using Newtonsoft.Json.Linq;

namespace ClientApplication.ServerConnection;

public class ChatMessage : ICommandHandler
{
	public void HandleCommand(Client client, JObject ob)
	{
		VRClient? vrClient = App.GetVrClientInstance();

		string? sender = ob["data"]?["sender"]?.ToObject<string>();
		string? message = ob["data"]?["message"]?.ToObject<string>();
		if (message != null)
		{
			DataViewModel data = new DataViewModel();
			data.AddMessage(message);
			if(vrClient != null && vrClient.PanelController != null)
				vrClient.PanelController.UpdateChat(sender, message);
		}
	}
}