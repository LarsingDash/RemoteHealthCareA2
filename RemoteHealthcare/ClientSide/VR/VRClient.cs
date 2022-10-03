using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using ClientSide.VR.CommandHandlers;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;

namespace ClientSide.VR;

public class VRClient
{
    private TcpClient _tcpClient = new();
    private NetworkStream _stream;

    private Dictionary<String, CommandHandler> commands = new();

    private byte[] _totalBuffer = new byte[0];
    private readonly byte[] _buffer = new byte[1024];

    public string tunnelID { get; private set; }
    public Tunnel tunnel { get; }

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

        //Set height values for tiles
        var noiseGen = new DotnetNoise.FastNoise();
        string heights = "";
        for (float X = 0; X < 256; X++)
        {
            for (float Y = 0; Y < 256; Y++)
            {
                heights += noiseGen.GetPerlin(X, Y) + ",";
            }
        }

        heights = heights.Substring(0, heights.Length - 1);

        //Add terain
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\AddTerrain", new Dictionary<string, string>()
            {
                {"\"_size1_\"", "256"},
                {"\"_size2_\"", "256"},
                {"\"_heights_\"", heights}
            })},
        });

        //Add node
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\AddNodeScene", new Dictionary<string, string>())},
        });
        //Add house
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\House", new Dictionary<string, string>())},
        });
        //Add car
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\WhiteCar", new Dictionary<string, string>())},
        });
        //Add Tree
        tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Tree", new Dictionary<string, string>())},
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

    /// <summary>
    /// It takes a string, converts it to a byte array, and sends it to the server
    /// </summary>
    /// <param name="String">The string to send to the server</param>
    public void SendData(String s)
    {
        Console.WriteLine($"Sending data: {s}");
        Byte[] data = BitConverter.GetBytes(s.Length);
        Byte[] comman = System.Text.Encoding.ASCII.GetBytes(s);
        _stream.Write(data, 0, data.Length);
        _stream.Write(comman, 0, comman.Length);
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