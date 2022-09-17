using System.Threading.Channels;
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

    
    /// <summary>
    /// SendTunnelMessage sends a message to the VR client
    /// </summary>
    /// <param name="values"><para>A dictionary of key-value pairs that will be sent to the server. Needs to have the following values:</para>
    /// <para>_tunnelID_ = destination of the tunnel</para>
    /// <para>"_data_" = data of what to do. Example {"id": "scene/terrain/add", "value1":2}</para>
    /// </param>
    public void SendTunnelMessage(Dictionary<string, string> values)
    {
        values.Add("_tunnelID_", _vrClient.tunnelID);
        _vrClient.SendData(JsonFileReader.GetObjectAsString("SendTunnel", values));
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
    }
}

/* Defining an enum that is used to identify the type of data that is being sent. */
public enum TunnelDataType : ushort
{
    Scene = 1,
}