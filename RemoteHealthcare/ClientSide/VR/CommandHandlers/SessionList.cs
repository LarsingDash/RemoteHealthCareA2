using System.Globalization;
using Newtonsoft.Json.Linq;

namespace ClientSide.VR.CommandHandlers;

public class SessionList : ICommandHandler
{
    private JObject currentObject = null;
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
            //Console.WriteLine(o["clientinfo"]["host"].ToObject<string>().ToLower() + " | " + Environment.MachineName.ToLower() + " | " + o["clientinfo"]["user"].ToObject<string>().ToLower() + " | " + Environment.UserName.ToLower());
            if (o["clientinfo"]["host"].ToObject<string>().ToLower().Contains(Environment.MachineName.ToLower()) &&
                o["clientinfo"]["user"].ToObject<string>().ToLower().Contains(Environment.UserName.ToLower()))
            {
                if (currentObject == null)
                {
                    currentObject = o;
                    parsedDate = CustomParseDate(o);
                }
                else
                {
                    if (parsedDate < CustomParseDate(o))
                    {
                        Console.WriteLine(parsedDate);
                        Console.WriteLine(CustomParseDate(o));
                        currentObject = o;
                        parsedDate = CustomParseDate(o);
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

    private DateTime CustomParseDate(JObject o)
    {
        return DateTime.ParseExact(o["lastPing"].ToObject<string>(), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
    }
}