using Newtonsoft.Json.Linq;

namespace ClientSide.VR.CommandHandlers;

public class SessionList : CommandHandler
{
    private JObject currentObject;
    private DateTime parsedDate;
    public void handleCommand(VRClient client, JObject ob)
    {
        //Console.WriteLine(ob.ToString());
        foreach (JObject o in ob["data"])
        {
            if (o["clientinfo"]["host"].ToObject<string>() == Environment.MachineName &&
                o["clientinfo"]["user"].ToObject<string>() == Environment.UserName)
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
                        currentObject = o;
                        parsedDate = DateTime.Parse(o["lastPing"].ToObject<string>());
                    }
                }
            }
            //Console.WriteLine(o["id"]);
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