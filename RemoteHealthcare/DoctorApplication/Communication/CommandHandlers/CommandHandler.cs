using Newtonsoft.Json.Linq;

namespace DoctorApplication.Communication.CommandHandlers;

public interface CommandHandler
{

    public void HandleCommand(Client client, JObject ob);

}