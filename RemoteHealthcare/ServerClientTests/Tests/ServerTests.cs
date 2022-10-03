using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json;
using ServerApplication;
using Shared;
using Shared.Log;

namespace ServerClientTests
{
    public class ServerTests
    {
        private int port = 2449;
        
        private Server server;
        
        /// <summary>
        /// The function sets up the server by creating a new server object and then waiting for half a second
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            server = new Server(port);

            Thread.Sleep(500);
        }

        /// <summary>
        /// This function tests if the server is connected to the correct port
        /// </summary>
        [Test]
        public void ConnectedToCorrectPort()
        {
            var tcpListener = server.GetFieldValue<TcpListener>("listener");
            Assert.That(int.Parse(tcpListener.LocalEndpoint.ToString()!.Split(":")[1]), Is.EqualTo(port), "The port of the server is not correct");
            Assert.Pass("The port of the server is connected to the right port!");
        }
        
        /// <summary>
        /// This function tests to see if the server has a public RSA key
        /// </summary>
        [Test]
        public void RsaKeyCorrect()
        {
            var rsa = server.GetFieldValue<RSA>("Rsa");
            Assert.NotNull(rsa.ExportRSAPublicKey(), "Server does not have a public RSA key.");
            Assert.Pass("Server does have a public RSA key.");
        }

        /// <summary>
        /// "This function tests whether the server can receive incoming requests from a client."
        /// 
        /// The first thing we do is create a new client connection. This is the connection that will be used to send
        /// requests to the server. We pass in a callback function that will be called whenever the client receives a
        /// message from the server
        /// </summary>
        [Test]
        public void ReceivingIncomingRequests()
        {
            DefaultClientConnection connection = new DefaultClientConnection("127.0.0.1", port, (ob, encrypted) => {
                Logger.LogMessage(LogImportance.Debug, "Receiving message: " + LogColor.Gray + "\n" + ob.ToString(Formatting.None));
            });
            Task.Delay(1000).ContinueWith((o) =>
            {
                connection.Disconnect();
            });
            Thread.Sleep(10);
            Assert.That(server.users.Count, Is.GreaterThan(0), "Cannot receive any incoming requests at server socket.");
            Assert.Pass("Server was able to make a connection");
        }
        
        
    }
}