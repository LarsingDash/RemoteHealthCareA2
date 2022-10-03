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
        
        [OneTimeSetUp]
        public void Setup()
        {
            server = new Server(port);

            Thread.Sleep(500);
        }

        [Test]
        public void ConnectedToCorrectPort()
        {
            var tcpListener = server.GetFieldValue<TcpListener>("listener");
            Assert.That(int.Parse(tcpListener.LocalEndpoint.ToString()!.Split(":")[1]), Is.EqualTo(port), "The port of the server is not correct");
            Assert.Pass("The port of the server is connected to the right port!");
        }
        
        [Test]
        public void RsaKeyCorrect()
        {
            var rsa = server.GetFieldValue<RSA>("Rsa");
            Assert.NotNull(rsa.ExportRSAPublicKey(), "Server does not have a public RSA key.");
            Assert.Pass("Server does have a public RSA key.");
        }

        [Test]
        public void ReceivingIncomingRequests()
        {
            DefaultClientConnection connection = new DefaultClientConnection("127.0.0.1", port, ob => {
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