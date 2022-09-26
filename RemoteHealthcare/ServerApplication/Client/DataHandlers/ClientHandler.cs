using ServerApplication.Client.DataHandlers.CommandHandlers;

namespace ServerApplication.Client.DataHandlers
{
    public class ClientHandler : DataHandler
    {

    
        public ClientHandler(ClientData clientData) : base(clientData)
        {
            CommandHandler = new()
            {
                {"public-rsa-key", new RsaKey()},
                {"encryptedMessage", new EncryptedMessage(clientData.Server.Rsa)},
                {"changedata", new ChangeData()}
            };
        }
    }
}