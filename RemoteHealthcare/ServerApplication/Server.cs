using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using ServerApplication.Client;
using Shared.Log;

namespace ServerApplication
{
    public class Server
    {
        #region Managing clients
        private TcpListener listener;
        public readonly List<ClientData> users = new();
        public readonly RSA Rsa = new RSACryptoServiceProvider();
        private Thread requestThread;
        #endregion

        public readonly Dictionary<string, List<ClientData>> SubscribedSessions = new();
        public readonly List<string> ActiveSessions = new();


        public Server(int port = 2460)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            requestThread = new Thread(start: () =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    Logger.LogMessage(LogImportance.Information, "Waiting for connection with client.");
                    TcpClient client = listener.AcceptTcpClient();
                    Logger.LogMessage(LogImportance.Information, "Accepted connection with client.");
                    users.Add(new ClientData(this, client));
                }
            });
            requestThread.Start();
        }

        /// <summary>
        /// It returns the public key of the RSA object
        /// </summary>
        /// <returns>
        /// The public key of the RSA object.
        /// </returns>
        public string GetRsaPublicKey()
        {
            return Rsa.ToXmlString(false);
        }

        public void RemoveUser(ClientData clientData)
        {
            users.Remove(clientData);
        }

        public ClientData? GetUser(string userName)
        {
            return users.FirstOrDefault(u => u.UserName.Equals(userName));
        }
    }
}