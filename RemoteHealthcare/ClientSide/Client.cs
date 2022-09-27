using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ClientSide
{
    internal class Client
    {
        private static TcpClient client;
        private static NetworkStream stream;
        private string userID;

        // private static byte[] totalBuffer = Array.Empty<byte>();
        private static string totalBuffer = "";
        private static readonly byte[] buffer = new byte[1024];

        private RSA rsa = new RSACryptoServiceProvider();
        private UnicodeEncoding encoder = new();

        private Dictionary<string, Action<JObject>> serialCallbacks = new();
        
        private byte[] publicKey;


        //TODO: Add a method to send a message to the server
        //TODO: Add a method to receive a message from the server

        public Client()
        {
            try
            {
                Console.WriteLine("Connecting to server...");
                client = new TcpClient();
                client.BeginConnect("localhost", 2460, new AsyncCallback(OnConnect), null);

                //TODO: Replace with login screen after login is implemented
                Console.WriteLine("Please enter your phone number: ");
                userID = Console.ReadLine();

                Console.WriteLine("Waiting for session to start");
                while (true)
                {
                    Console.WriteLine("Insert Server Command:");
                    string command = Console.ReadLine();
                    switch (command)
                    {  
                        case "stop":
                            Console.WriteLine("Stopping client");
                            CloseConnection();
                            break;
                        case "start":
                            string payload = Console.ReadLine();
                            write($"chat\r\n{payload}");
                            break;
                    }
                }
            }
            catch(Exception exception)
            {
                Console.WriteLine($"Could not connect with server... {exception}");
            }
        }
        
        private static void OnConnect(IAsyncResult ar)
        {
            client.EndConnect(ar);
            Console.WriteLine("Connected to server!");
            stream = client.GetStream();
            stream.BeginRead(buffer, 0, 1024, OnRead, null);
            
        }

        private static void OnRead(IAsyncResult ar)
        {
            int receivedBytes = stream.EndRead(ar);
            string receivedText = System.Text.Encoding.ASCII.GetString(buffer, 0, receivedBytes);
            totalBuffer += receivedText;

            while (totalBuffer.Contains("\r\n\r\n"))
            {
                string packet = totalBuffer.Substring(0, totalBuffer.IndexOf("\r\n\r\n"));
                totalBuffer = totalBuffer.Substring(totalBuffer.IndexOf("\r\n\r\n") + 4);
                string[] packetData = Regex.Split(packet, "\r\n");
                dataHandler(packetData);
            }
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnRead), null);
        }

        private static void write(string data)
        {
            var dataAsBytes = System.Text.Encoding.ASCII.GetBytes(data + "\r\n\r\n");
            stream.Write(dataAsBytes, 0, dataAsBytes.Length);
            stream.Flush();
        }

        private static void dataHandler(string[] packetData)
        {
            Console.WriteLine("Received Data: " + packetData[0]);
            
            switch (packetData[0])
            {
                case "message":
                    Console.WriteLine(packetData[1]);
                    break;
            }
        }

        private static void CloseConnection()
        {
            stream.Close();
            client.Close();
        }
    }
}
