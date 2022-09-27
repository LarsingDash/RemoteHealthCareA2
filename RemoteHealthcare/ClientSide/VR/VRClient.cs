using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using ClientSide.VR.CommandHandlers;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClientSide.VR;

public class VRClient
{
    private TcpClient _tcpClient = new();
    private NetworkStream _stream;

    private Dictionary<String, CommandHandler> commands = new();

    private byte[] _totalBuffer = new byte[0];
    private readonly byte[] _buffer = new byte[1024];

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
        commands.Add("session/list", new SessionList());
        commands.Add("tunnel/create", new TunnelCreate());
        commands.Add("tunnel/send", tunnel = new Tunnel(this));
    }

    /// <summary>
    /// It creates a tunnel with the given id
    /// </summary>
    /// <param name="id">The ID of the tunnel you want to create.</param>
    public void createTunnel(string id)
    {
        Console.WriteLine($"ID: {id}");
        SendData(JsonFileReader.GetObjectAsString("CreateTunnel", new Dictionary<string, string>()
        {
            {"_id_", id}
        }));
    }

    /// <summary>
    /// Its sets the tunnelID
    /// </summary>
    /// <param name="id">The id of the tunnel.</param>
    /// <returns>
    /// The tunnel id is being returned.
    /// </returns>
    public void setTunnelID(string id)
    {
        tunnelID = id;
        Console.WriteLine($"Received tunnel id: {id}");

        //Remove Default Objects
        RemoveObjectRequest("GroundPlane", "Head", "RightHand", "LeftHand");
            
        //Start WorldGen
        worldGen = new WorldGen(this, tunnel, selectedWorld);
        
        //Start HUDController
        panelController = new PanelController(this, tunnel);
    }

        //Add node
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\AddNodeScene", new Dictionary<string, string>())},
        });

        tunnel.Subscribe(TunnelDataType.Scene, ob =>
        {
            Console.WriteLine(ob);
            try
            {
                JObject foundObject = ob["data"]["data"]["children"].First(o =>
                {
                    if (o["name"].ToObject<string>().Equals("GroundPlane"))
                    {
                        return true;
                    }

                    return false;
                }).ToObject<JObject>();

                string uuid = foundObject["uuid"].ToObject<string>();
                Console.WriteLine($"UUID: {uuid}");
                tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {
                        "\"_data_\"",
                        JsonFileReader.GetObjectAsString("TunnelMessages\\DeleteNodeScene",
                            new Dictionary<string, string>())
                    },
                    {"_id_", uuid}
                });
            }
            catch
            {
                Console.WriteLine("No GroundPlane found, already removed?");
            }

        });
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\GetScene", new Dictionary<string, string>())},
            }
        );

        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\SetTimeScene", new Dictionary<string, string>())},
            {"\"_time_\"", "15.0"}
        });
    }
    /// <summary>
    /// It connects to the server, gets the stream, and starts reading the stream
    /// </summary>
    public async Task StartConnectionAsync()
    {
        try
        {
            await _tcpClient.ConnectAsync("145.48.6.10", 6666);
            _stream = _tcpClient.GetStream();
            _stream.BeginRead(_buffer, 0, 1024, onRead, null);

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

    /// <summary>
    /// It takes a JObject, converts it to a string, and then sends it to the SendData function that takes a string.
    /// </summary>
    /// <param name="JObject">The data you want to send.</param>
    public void SendData(JObject j)
    {
        SendData(j.ToString());
    }

    /// <summary>
    /// It reads data from the stream, and if it has enough data to read a packet, it reads the packet and calls the
    /// appropriate command handler
    /// </summary>
    /// <param name="IAsyncResult">This is the result of the async operation.</param>
    /// <returns>
    /// The data that was sent from the server.
    /// </returns>
    private void onRead(IAsyncResult ar)
    {
        try
        {
            int rc = _stream.EndRead(ar);
            _totalBuffer = Concat(_totalBuffer, _buffer, rc);
        }
        catch (System.IO.IOException)
        {
            Console.WriteLine("Error");
            return;
        }
        while (_totalBuffer.Length >= 4)
        {
            int packetSize = BitConverter.ToInt32(_totalBuffer, 0);
            if (_totalBuffer.Length >= packetSize + 4)
            {
                string data = Encoding.UTF8.GetString(_totalBuffer, 4, packetSize);
                JObject jData = JObject.Parse(data);
                //Console.WriteLine(jData.ToString());
                if (commands.ContainsKey(jData["id"].ToObject<string>()))
                {
                    commands[jData["id"].ToObject<string>()].handleCommand(this, jData);
                }
                else
                {
                    Console.WriteLine($"Could not find command for {jData["id"]}");
                }
                var newBuffer = new byte[_totalBuffer.Length - packetSize - 4];
                Array.Copy(_totalBuffer, packetSize + 4, newBuffer, 0, newBuffer.Length);
                _totalBuffer = newBuffer;
            }
            else
                break;
        }
        _stream.BeginRead(_buffer, 0, 1024, onRead, null);
    }

    /// <summary>
    /// It takes two byte arrays and a count, and returns a new byte array that is the concatenation of the first two
    /// arrays, with the second array truncated to the specified count
    /// </summary>
    /// <param name="b1">The first byte array to concatenate.</param>
    /// <param name="b2">The byte array to be appended to b1</param>
    /// <param name="count">The number of bytes to copy from the second array.</param>
    /// <returns>
    /// The concatenated byte array.
    /// </returns>
    private static byte[] Concat(byte[] b1, byte[] b2, int count)
    {
        byte[] r = new byte[b1.Length + count];
        System.Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
        System.Buffer.BlockCopy(b2, 0, r, b1.Length, count);
        return r;
    }
}