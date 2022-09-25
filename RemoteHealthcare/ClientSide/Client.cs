using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientSide
{
    internal class Client
    {
        private TcpClient client;
        private NetworkStream stream;
        private string userID;

        
        //TODO: Add a method to send a message to the server
        //TODO: Add a method to receive a message from the server

        public Client()
        {
            try
            {
                Console.WriteLine("Connecting to server...");
                client = new TcpClient("145.49.45.129", 2460);
                stream = client.GetStream();
                Console.WriteLine("Connected to server!");
                
                //TODO: Replace with login screen after login is implemented
                Console.WriteLine("Please enter your phone number: ");
                userID = Console.ReadLine();

                Console.WriteLine("Waiting for session to start");
                while (true)
                {
                    Thread.Sleep(5);
                }
            }
            catch(Exception exception)
            {
                Console.WriteLine($"Could not connect with server... {exception}");
            }
        }
        
        public void CloseConnection()
        {
            stream.Close();
            client.Close();
        }
    }
}
