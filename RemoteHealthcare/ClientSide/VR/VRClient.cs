using System.Net;
using System.Net.Sockets;
using System.Text;
using ClientSide.VR.CommandHandlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClientSide.VR;

public class VRClient
{
    private TcpClient _tcpClient = new TcpClient();
    private NetworkStream _stream;

    private Dictionary<String, CommandHandler> commands = new();
    //private string tunnelString;
    
    private byte[] _totalBuffer = new byte[0];
    private readonly byte[] _buffer = new byte[1024];

    private string tunnelID = null;

    public VRClient()
    {
        commands.Add("session/list", new SessionList());
        commands.Add("tunnel/create", new TunnelCreate());
    }

    public void createTunnel(string id)
    {
        Console.WriteLine($"ID: {id}");
        SendData($@"{{""id"": ""tunnel/create"", ""data"":{{""session"":""{id}"", ""key"":""""}}}}");
    }

    public void setTunnelID(string id)
    {
        tunnelID = id;
        Console.WriteLine($"Received tunnel id: {id}");
    }
    public async Task StartConnectionAsync()
    {
        Console.WriteLine("VRClient started");
        try
        {
            Console.WriteLine("Trying to connect");
            var ip = IPAddress.Parse("145.48.6.10");
            await _tcpClient.ConnectAsync(ip, 6666);
            Console.WriteLine("Connected");
            _stream = _tcpClient.GetStream();
            _stream.BeginRead(_buffer, 0, 1024, onRead, null);
            
            SendData(@"{""id"": ""session/list""}");
        }
        catch
        {
            Console.WriteLine("Could not connect with VRServer...");
        }
    }

    public void SendData(String s)
    {
        Console.WriteLine($"Sending data: {s}");
        Byte[] data = BitConverter.GetBytes(s.Length);
        Byte[] comman = System.Text.Encoding.ASCII.GetBytes(s);
        _stream.Write(data, 0, data.Length);
        _stream.Write(comman, 0, comman.Length);
    }

    public void SendData(JObject j)
    {
        SendData(j.ToString());
    }
    
    private void onRead(IAsyncResult ar)
    {
        try
        {
            int rc = _stream.EndRead(ar);
            _totalBuffer = Concat(_totalBuffer, _buffer, rc);
        }
        catch(System.IO.IOException)
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
                if(commands.ContainsKey(jData["id"].ToObject<string>()))
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
    
    private static byte[] Concat(byte[] b1, byte[] b2, int count)
    {
        byte[] r = new byte[b1.Length + count];
        System.Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
        System.Buffer.BlockCopy(b2, 0, r, b1.Length, count);
        return r;
    }
    
    
    
}