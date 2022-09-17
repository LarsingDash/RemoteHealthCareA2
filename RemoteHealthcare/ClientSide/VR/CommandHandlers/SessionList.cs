using Newtonsoft.Json.Linq;

namespace ClientSide.VR.CommandHandlers;

public class SessionList : CommandHandler
{
    private JObject currentObject;
    private DateTime parsedDate;
    /// <summary>
    /// It loops through all the clients in the JSON object, and if the client is on the same machine and has the same
    /// username as the current user, it will set the currentObject to that client
    /// </summary>
    /// <param name="VRClient">The client that is handling the command.</param>
    /// <param name="JObject">The JSON object that was sent from the server.</param>
    public void handleCommand(VRClient client, JObject ob)
    {
        foreach (JObject o in ob["data"])
        {
            if (o["clientinfo"]["host"].ToObject<string>().ToLower() == Environment.MachineName.ToLower() &&
                o["clientinfo"]["user"].ToObject<string>().ToLower() == Environment.UserName.ToLower())
            {
                if (currentObject == null)
                {
                    currentObject = o;
                    parsedDate = DateTime.Parse(o["lastPing"].ToObject<string>());
                }
                else
                {
                    if (parsedDate < DateTime.Parse(o["lastPing"].ToObject<string>()))
                    {
                        Console.WriteLine(parsedDate);
                        Console.WriteLine(DateTime.Parse(o["lastPing"].ToObject<string>()));
                        currentObject = o;
                        parsedDate = DateTime.Parse(o["lastPing"].ToObject<string>());
                    }
                }
            }
        }

        if (currentObject != null)
        {
            client.createTunnel(currentObject["id"].ToObject<string>());
        }
        else
        {
            Console.WriteLine("Could not find user...");
        }
    }
}