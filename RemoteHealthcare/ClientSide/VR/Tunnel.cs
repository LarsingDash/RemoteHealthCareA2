using ClientSide.VR.CommandHandlers;
using ClientSide.VR.CommandHandlers.TunnelMessages;
using Newtonsoft.Json.Linq;

namespace ClientSide.VR;

public class Tunnel : CommandHandler
{
    private VRClient _vrClient;
    private Dictionary<string, CommandHandler> _commands = new();
    private Dictionary<TunnelDataType, JObject> _data = new();
    private Dictionary<TunnelDataType, List<Action<JObject>>> _observers = new ();
    /* Initializing the tunnel class. */
    public Tunnel(VRClient vrClient)
    {
        _vrClient = vrClient;
        _commands.Add("scene/get", new GetScene());
        _commands.Add("scene/skybox/settime", new SetTimeScene());
        
        foreach (TunnelDataType u in Enum.GetValues(typeof(TunnelDataType)))
        {
            _observers.Add(u, new List<Action<JObject>>());
        }
    }

    //Helper method to send tunnelMessages without having to add the tunnelID
    public void SendTunnelMessage(Dictionary<string, string> values, bool silent = false)
    {
        values.Add("_tunnelID_", vrClient.TunnelID);
        vrClient.SendData(JsonFileReader.GetObjectAsString("SendTunnel", values), silent);
    }


    /// <summary>
    /// It checks if the command exists in the dictionary, and if it does, it calls the handleCommand function of the
    /// command
    /// </summary>
    /// <param name="VRClient">The client that sent the command</param>
    /// <param name="JObject">The JSON object that was received from the server.</param>
    public void handleCommand(VRClient client, JObject ob)
    {
        if (_commands.ContainsKey(ob["data"]["data"]["id"].ToObject<string>()))
        {
            _commands[ob["data"]["data"]["id"].ToObject<string>()].handleCommand(client, ob["data"].ToObject<JObject>());
        }
        else
        {
            Console.WriteLine($"SendTunnel, no command for {ob["data"]["data"]}");
        }
    }

    /// <summary>
    /// It takes a TunnelDataType and a JObject, and then it updates the value of the TunnelDataType in the _data
    /// dictionary, and then it calls all the observers of that TunnelDataType. When its done it removes all the observers.
    /// </summary>
    /// <param name="TunnelDataType">This is an enum that you can define yourself. It's used to identify the type of data
    /// you're sending.</param>
    /// <param name="JObject">This is the data type that is used to send data through the tunnel. It is a JSON
    /// object.</param>
    public void UpdateValue(TunnelDataType type, JObject value)
    {
        _data[type] = value;
        foreach (Action<JObject> ob in _observers[type])
        {
            ob(value);
        }

        _observers[type] = new List<Action<JObject>>();
    }
    
    /// <summary>
    /// > Subscribe to a specific type of data from the server
    /// </summary>
    /// <param name="TunnelDataType">This is an enum that is used to identify the type of data that is being sent.</param>
    /// <param name="ob">The method that will be called when the data is received.</param>
    public void Subscribe(TunnelDataType type, Action<JObject> ob)
    {
        if (!_observers[type].Contains(ob)) {
            _observers[type].Add(ob);
        }
    }
    /// <summary>
    /// > Unsubscribe from a specific type of data
    /// </summary>
    /// <param name="TunnelDataType">This is an enum that defines the type of data that is being sent.</param>
    /// <param name="ob">The method that will be called when the data is received.</param>
    public void UnSubscribe(TunnelDataType type, Action<JObject> ob)
    {
        if (_observers[type].Contains(ob)) {
            _observers[type].Remove(ob);
        }

        //Handle response according to message ID
        switch (messageID)
        {
            default:
                Console.WriteLine("Message ID not recognized from JSON:");
                Console.WriteLine(json);
                break;

            case "session/list":
                vrClient.ListSessions(json);
                break;

            case "tunnel/create":
                string? sessionID = json["data"]["id"].ToObject<string>();

                if (sessionID == null)
                {
                    Console.WriteLine("SessionID was null");
                    break;
                }

                vrClient.TunnelStartup(sessionID);
                break;

            case "scene/get":
                Console.WriteLine(json);
                vrClient.RemoveObject(json);
                break;
            
            case "scene/node/add":
                var nodeName = json["data"]["data"]["data"]["name"].ToObject<string>();
                var nodeId = json["data"]["data"]["data"]["uuid"].ToObject<string>();
                Console.WriteLine($"Added: {nodeName} with uuid {nodeId}");

                if (nodeName != null && nodeId != null)
                {
                    vrClient.SavedIDs.Add(nodeName, nodeId);
                    if (vrClient.IDWaitList.ContainsKey(nodeName))
                    {
                        Console.WriteLine("Running Action:");
                        vrClient.IDWaitList[nodeName].Invoke(nodeId);
                    }
                }
                break;
            
            case "scene/terrain/add":
                vrClient.worldGen.PathGen();
                break;
            
            case "route/add":
                var routeId = json["data"]["data"]["data"]["uuid"].ToObject<string>();
                Console.WriteLine($"Added: route with uuid {routeId}");
                string routeName = "route";
                if (routeId != null)
                {
                    vrClient.SavedIDs.Add(routeName, routeId);
                    if (vrClient.IDWaitList.ContainsKey(routeName))
                    {
                        Console.WriteLine("Running Action:");
                        vrClient.IDWaitList[routeName].Invoke(routeId);
                    }
                }

                vrClient.worldGen.AnimateBike();
                break;
            
            case "scene/node/find":
                try
                {
                    Console.WriteLine(json);
                    var foundName = json["data"]["data"]["data"][0]["name"].ToObject<string>();
                    var foundID = json["data"]["data"]["data"][0]["uuid"].ToObject<string>();
                    Console.WriteLine($"Found: {foundName} with uuid {foundID}");

                    if (foundName != null && foundID != null)
                    {
                        if (vrClient.IDSearchList.ContainsKey(foundName))
                        {
                            vrClient.IDSearchList[foundName].Invoke(foundID);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                break;
        }
        Console.WriteLine("------------------------------------------------------------Response End");
    }
}

/* Defining an enum that is used to identify the type of data that is being sent. */
public enum TunnelDataType : ushort
{
    Scene = 1,
}