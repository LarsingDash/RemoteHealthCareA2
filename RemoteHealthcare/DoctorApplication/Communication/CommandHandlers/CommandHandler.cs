using Newtonsoft.Json.Linq;

namespace DoctorApplication.Communication.CommandHandlers;

public interface ICommandHandler
{
    public void HandleCommand(Client client, JObject ob);
}