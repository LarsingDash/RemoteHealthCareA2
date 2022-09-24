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
    private RSA rsa = new RSACryptoServiceProvider();

    public delegate void MessageReceived(ClientData client, JObject json);
    public MessageReceived OnMessage { get; }

    private List<ClientData> _users = new();
    
    private Dictionary<string, Action<JObject>> serialCallbacks = new();
    private Dictionary<string, ICommandHandler> commandHandlers = new();

    public Server()
    {
        OnMessage = HandleMessage;
        
        commandHandlers.Add("public-rsa-key", new RSAKey());
        commandHandlers.Add("encryptedMessage", new EncryptedMessage(rsa));
        
        _listener = new TcpListener(IPAddress.Any, 2460);
        _listener.Start();
        while (true)
        {
            Logger.LogMessage(LogImportance.Information, "Waiting for connection with client.");
            TcpClient client = _listener.AcceptTcpClient();
            Logger.LogMessage(LogImportance.Information, "Accepted connection with client.");
            _users.Add(new ClientData(this, client));
        }
    }

    private void HandleMessage(ClientData clientData, JObject json)
    {
        if (!json.ContainsKey("id"))
        {
            Logger.LogMessage(LogImportance.Error, $"Got message with no id from {clientData.UserName}: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
            return;
        }
        if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
        {
            Logger.LogMessage(LogImportance.Information, $"Got message from {clientData.UserName}: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
        }

        if (json.ContainsKey("serial"))
        {
            var serial = json["serial"]!.ToObject<string>();
            if (serialCallbacks.ContainsKey(serial!))
            {
                serialCallbacks[serial!].Invoke(json);
                serialCallbacks.Remove(serial!);
                return;
            }
        }

        if (commandHandlers.ContainsKey(json["id"]!.ToObject<string>()!))
        {
            commandHandlers[json["id"]!.ToObject<string>()!].HandleMessage(this, clientData, json);
        }
        else
        {
            Logger.LogMessage(LogImportance.Warn, $"Got message from {clientData.UserName} but no commandHandler found: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
        }
        
        
    }

    public byte[] GetRsaPublicKey()
    {
        return rsa.ExportRSAPublicKey();
    }

    public void Broadcast()
    {
        //TODO Send message to all clients.
    }

    public void AddSerialCallback(string serial, Action<JObject> action)
    {
        if (serialCallbacks.ContainsKey(serial))
        {
            serialCallbacks.Remove(serial);
        }
        
        serialCallbacks.Add(serial, action);
    }
}