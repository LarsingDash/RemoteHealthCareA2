using ClientApplication.Model;
using ClientSide.VR2;
using Newtonsoft.Json.Linq;

namespace ClientApplication.ServerConnection;

public class ChatMessage : ICommandHandler
{
	public void HandleCommand(Client client, JObject ob)
	{
		VRClient vrClient = App.GetVrClientInstance();
		
		string? message = ob["data"]?["message"]?.ToObject<string>();
		if (message != null)
		{
			DataViewModel data = new DataViewModel();
			data.AddMessage(message);
			vrClient.PanelController.UpdateChat(message);
		}
	}
}