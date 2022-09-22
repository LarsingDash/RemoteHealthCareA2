using System;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ServerApplication;

public class ClientHandler
{
    public Server Server { get; }
    public TcpClient Client { get; }
    public NetworkStream Stream { get; }
    
    private byte[] _totalBuffer = Array.Empty<byte>();
    private readonly byte[] _buffer = new byte[1024];
    
    public ClientHandler(Server server, TcpClient client)
    {
        this.Server = server;
        this.Client = client;
        this.Stream = client.GetStream();
        Stream.BeginRead(_buffer, 0, 1024, OnRead, null);
        
    }
    
    private void OnRead(IAsyncResult readResult)
    {
        try
        {
            var numberOfBytes = Stream.EndRead(readResult);
            _totalBuffer = Concat(_totalBuffer, _buffer, numberOfBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
            return;
        }

        while (_totalBuffer.Length >= 4)
        {
            var packetSize = BitConverter.ToInt32(_totalBuffer, 0);

            if (_totalBuffer.Length >= packetSize + 4)
            {
                var json = Encoding.UTF8.GetString(_totalBuffer, 4, packetSize);
                Server.OnMessage(Client, JObject.Parse(json));

                var newBuffer = new byte[_totalBuffer.Length - packetSize - 4];
                Array.Copy(_totalBuffer, packetSize + 4, newBuffer, 0, newBuffer.Length);
                _totalBuffer = newBuffer;
            }

            else
                break;
        }

        Stream.BeginRead(_buffer, 0, 1024, OnRead, null);
    }
    
    private static byte[] Concat(byte[] b1, byte[] b2, int count)
    {
        var r = new byte[b1.Length + count];
        Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
        Buffer.BlockCopy(b2, 0, r, b1.Length, count);
        return r;
    }
}