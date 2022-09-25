using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using ClientSide.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerApplication;
using ServerSide.CommandHandlers;
using SharedProject;
using SharedProject.Log;


namespace ServerSide;

public class Server
{
    private TcpListener _listener;
    public readonly RSA Rsa = new RSACryptoServiceProvider();

    private List<ClientData> users = new();

    public Server()
    {

        _listener = new TcpListener(IPAddress.Any, 2460);
        _listener.Start();
        while (true)
        {
            Logger.LogMessage(LogImportance.Information, "Waiting for connection with client.");
            TcpClient client = _listener.AcceptTcpClient();
            Logger.LogMessage(LogImportance.Information, "Accepted connection with client.");
            users.Add(new ClientData(this, client));
        }
    }

    public byte[] GetRsaPublicKey()
    {
        return Rsa.ExportRSAPublicKey();
    }

    public void Broadcast()
    {
        //TODO Send message to all clients.
    }
}