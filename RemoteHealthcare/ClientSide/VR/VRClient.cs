using System.Globalization;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using Shared;

namespace ClientSide.VR;

public class VRClient
{
    //Session List
    private JObject savedSession;
    private DateTime savedSessionDate;

    //Tunnel
    private readonly TcpClient tcpClient = new TcpClient();
    private NetworkStream stream;
    public string TunnelID { get; private set; }
    private Tunnel tunnel { get; }

    //Data buffers for stream
    private byte[] totalBuffer = Array.Empty<byte>();
    private readonly byte[] buffer = new byte[1024];

    //VRClient
    public WorldGen worldGen;
    public PanelController panelController;
    private World selectedWorld = World.forest;

    //Other
    private readonly List<string> removalTargets = new List<string>();
    public readonly Dictionary<string, string> SavedIDs = new Dictionary<string, string>();
    public readonly Dictionary<string, Action<string>> IDWaitList = new Dictionary<string, Action<string>>();       //Waiting for it to be added
    public readonly Dictionary<string, Action<string>> IDSearchList = new Dictionary<string, Action<string>>();     //Waiting for it to be found

    public VRClient()
    {
        tunnel = new Tunnel(this);
    }

    //After the correct session has been located the tunnel will be created using the sessionID
    private void CreateTunnel(string id)
    {
        Console.WriteLine($"ID: {id}");
        SendData(JsonFileReader.GetObjectAsString("CreateTunnel", new Dictionary<string, string>()
        {
            { "_id_", id }
        }));
    }

    //Run startup actions after the tunnel has been created
    public void TunnelStartup(string id)
    {
        TunnelID = id;

        //Remove Default Objects
        RemoveObjectRequest("GroundPlane", "RightHand", "LeftHand");
            
        //Start WorldGen
        worldGen = new WorldGen(this, tunnel, selectedWorld);
        
        //Start HUDController
        panelController = new PanelController(this, tunnel);
    }

    //It connects to the server, gets the stream, and starts reading the stream. Then it asks for all sessions to find the correct one in the response
    public async Task StartConnectionAsync()
    {
        try
        {
            await tcpClient.ConnectAsync("145.48.6.10", 6666);
            stream = tcpClient.GetStream();
            stream.BeginRead(buffer, 0, 1024, OnRead, null);

            SendData(JsonFileReader.GetObjectAsString("SessionList", new Dictionary<string, string>()));
        }
        catch
        {
            Console.WriteLine("Could not connect with VRServer...");
        }
    }

    //Sends the message to the server by writing it to the streams (also prints it in the console). Use SendTunnelMessage() to include ID
    public void SendData(string text, bool silent = false)
    {
        Console.WriteLine("-------------------------------------------Send Start");
        if (!silent)
        {
            Console.WriteLine($"Sending data:\n{text}");
        }
        else
        {
            Console.WriteLine($"Sending data: (Silent)");
        }

        byte[] data = BitConverter.GetBytes(text.Length);
        byte[] command = Encoding.ASCII.GetBytes(text);

        stream.Write(data, 0, data.Length);
        stream.Write(command, 0, command.Length);

        Console.WriteLine("-------------------------------------------Send End");
    }

    //Reads the data from the stream and passes the json to the response handler
    private void OnRead(IAsyncResult asyncResult)
    {
        try
        {
            var readCount = stream.EndRead(asyncResult);
            totalBuffer = Concat(totalBuffer, buffer, readCount);
        }
        catch (IOException)
        {
            Console.WriteLine("OnRead Error");
            return;
        }

        while (totalBuffer.Length >= 4)
        {
            var packetSize = BitConverter.ToInt32(totalBuffer, 0);
            if (totalBuffer.Length >= packetSize + 4)
            {
                var data = Encoding.UTF8.GetString(totalBuffer, 4, packetSize);
                var jData = JObject.Parse(data);
                tunnel.HandleResponse(this, jData);
                var newBuffer = new byte[totalBuffer.Length - packetSize - 4];
                Array.Copy(totalBuffer, packetSize + 4, newBuffer, 0, newBuffer.Length);
                totalBuffer = newBuffer;
            }
            else
                break;
        }

        stream.BeginRead(buffer, 0, 1024, OnRead, null);
    }

    //Helper method for OnRead for occasions where the data is not properly formatted
    private static byte[] Concat(byte[] b1, byte[] b2, int count)
    {
        byte[] r = new byte[b1.Length + count];
        System.Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
        System.Buffer.BlockCopy(b2, 0, r, b1.Length, count);
        return r;
    }

    public void ListSessions(JObject json)
    {
        //Go through all sessions in the json and find the one matching this systems host and user
        foreach (JObject currentObject in json["data"])
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

        //If a session with the correct host and user was found create a tunnel
        if (savedSession != null)
        {
            CreateTunnel(savedSession["id"].ToObject<string>());
        }
        else
        {
            Console.WriteLine("Could not find user...");
        }
    }

    //Helper method for ListSessions()
    private DateTime CustomParseDate(JObject jsonTime)
    {
        return DateTime.ParseExact(jsonTime["lastPing"].ToObject<string>(), "MM/dd/yyyy HH:mm:ss",
            CultureInfo.InvariantCulture);
    }

    //Add the targeted object to the list of objects-to-remove and send a request to find that object
    private void RemoveObjectRequest(params string[] targets)
    {
        foreach (var target in targets) removalTargets.Add(target);

        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {
                "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\GetScene",
                    new Dictionary<string, string>())
            },
        });
    }

    //Loop through the json to find all objects that have a name that is in the removalTargets list, once a target has been found, request its removal
    public void RemoveObject(JObject json)
    {
        try
        {
            foreach (JObject currentObject in json["data"]["data"]["data"]["children"])
            {
                string? name = currentObject["name"].ToObject<string>();
                if (name == null) continue;

                if (!removalTargets.Contains(name)) continue;
                //Send a message to remove the node with the found uuid
                string? uuid = currentObject["uuid"].ToObject<string>();

                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\DeleteNodeScene",
                            new Dictionary<string, string>())
                    },
                    { "_id_", uuid }
                });
            }
        }
        catch
        {
            Console.WriteLine("No GroundPlane found, already removed?");
        }
    }
}