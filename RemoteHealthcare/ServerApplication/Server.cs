using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using ServerApplication.Client;
using ServerApplication.Log;

namespace ServerApplication
{
    public class Server
    {
        #region Managing clients
        private TcpListener listener;
        private List<ClientData> users = new();
        public readonly RSA Rsa = new RSACryptoServiceProvider();
        #endregion


        public Server()
        {

            listener = new TcpListener(IPAddress.Any, 2460);
            listener.Start();
            while (true)
            {
                Logger.LogMessage(LogImportance.Information, "Waiting for connection with client.");
                TcpClient client = listener.AcceptTcpClient();
                Logger.LogMessage(LogImportance.Information, "Accepted connection with client.");
                users.Add(new ClientData(this, client));
            }
        }

        /// <summary>
        /// It returns the public key of the RSA object
        /// </summary>
        /// <returns>
        /// The public key of the RSA object.
        /// </returns>
        public byte[] GetRsaPublicKey()
        {
            return Rsa.ExportRSAPublicKey();
        }

        public void Broadcast()
        {
            //TODO Send message to all clients.
        }

        public void RemoveUser(ClientData clientData)
        {
            users.Remove(clientData);
        }
    }
}