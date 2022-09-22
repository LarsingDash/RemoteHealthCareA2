using System.Net.Sockets;

namespace ServerApplication
{
    public class ClientData
    {
        public TcpClient TcpClient { get; }
        public string UserName { get; }

        private ClientHandler _handler;
        private readonly Server _server;
        

        public ClientData(Server server, TcpClient client)
        {
            TcpClient = client;
            UserName = "Unknown";
            _server = server;
            
            _handler = new ClientHandler(_server, client);
        }
    }
}