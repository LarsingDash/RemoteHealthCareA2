using System.Globalization;
using Newtonsoft.Json.Linq;
using Shared.Log;

namespace ClientSide.VR2.CommandHandler;

public class SessionList : ICommandHandlerVR
{
    
    private JObject savedSession;
    private DateTime savedSessionDate;
    public void HandleCommand(VRClient client, JObject ob)
    {
        foreach (JObject currentObject in ob["data"])
        {
            string? host = currentObject["clientinfo"]["host"].ToObject<string>();
            string? user = currentObject["clientinfo"]["user"].ToObject<string>();

            //Make sure neither are null
            if (host == null || user == null) continue;

            //Check if the host and user correspond to the systems host and user
            if (host.ToLower().Contains(Environment.MachineName.ToLower()) &&
                user.ToLower().Contains(Environment.UserName.ToLower()))
            {
                //Save the session object if there wasn't one saved already or if this one is newer
                if (savedSession == null)
                {
                    savedSession = currentObject;
                    savedSessionDate = CustomParseDate(currentObject);
                }
                else
                {
                    if (savedSessionDate < CustomParseDate(currentObject))
                    {
                        savedSession = currentObject;
                        savedSessionDate = CustomParseDate(currentObject);
                    }
                }
            }
        }
        if (savedSession != null)
        {
            client.CreateTunnel(savedSession["id"]!.ToObject<string>()!);
        }
        else
        {
            Logger.LogMessage(LogImportance.Fatal, "Could not find user in session list.");
            //TODO Stop VR?
        }
    }
    
    private DateTime CustomParseDate(JObject jsonTime)
    {
        return DateTime.ParseExact(jsonTime["lastPing"].ToObject<string>(), "MM/dd/yyyy HH:mm:ss",
            CultureInfo.InvariantCulture);
    }
}