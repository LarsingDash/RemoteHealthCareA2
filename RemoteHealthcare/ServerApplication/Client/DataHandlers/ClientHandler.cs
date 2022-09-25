using System.Security.Cryptography;
using ServerSide.CommandHandlers;

namespace ServerApplication.DataHandlers;

public class ClientHandler : DataHandler
{

    
    public ClientHandler(ClientData clientData) : base(clientData)
    {
        CommandHandler = new()
        {
            {"public-rsa-key", new RSAKey()},
            {"encryptedMessage", new EncryptedMessage(clientData.Server.Rsa)}
        };
    }
}