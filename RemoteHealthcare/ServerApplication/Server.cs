#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace ServerApplication
{
    
    public class Server
    {
        private TcpListener _listener;

        public delegate void MessageReceived(TcpClient client, JObject json);

        private List<ClientData> _users = new();
        public MessageReceived OnMessage { get; }

        public Server()
        {
            OnMessage = HandleMessage;
            _listener = new TcpListener(IPAddress.Any, 2460);
            _listener.Start();
            while (true)
            {
                Console.WriteLine("Waiting for connection");
                TcpClient client = _listener.AcceptTcpClient();
                Console.WriteLine("Accepted client");
                _users.Add(new ClientData(this, client));
            }
        }
        
        private void HandleMessage(TcpClient client, JObject json)
        {
            ClientData? data = GetClientDataByTcpClient(client);
            if (data == null)
            {
                Console.WriteLine($"Received message from unknown source: \n {json}");
                return;
            }
            switch (json["id"].ToObject<string>())
            {
                
            }
        
            string username = data.UserName != null ? data.UserName : "(GeenUserName)";
            Console.WriteLine($"Received message from {username}");
            Console.WriteLine(json);
        }
        
        private ClientData? GetClientDataByTcpClient(TcpClient client)
        {
            try
            {
                ClientData clientData = _users.First(u => u != null && u.TcpClient == client)!;
                return clientData;
            }
            catch
            {
                return null;
            }
        }
    }
}