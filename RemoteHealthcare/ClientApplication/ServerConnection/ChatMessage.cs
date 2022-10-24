using ClientApplication.Model;
using Newtonsoft.Json.Linq;

namespace ClientApplication.ServerConnection;

public class ChatMessage : ICommandHandler
{
	public void HandleCommand(Client client, JObject ob)
	{
		string? message = ob["data"]?["message"]?.ToObject<string>();
		if (message != null)
		{
			DataViewModel data = new DataViewModel();
			data.AddMessage(message);
		}
	}
}