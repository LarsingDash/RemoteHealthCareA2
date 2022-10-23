using ServerApplication.Client.DataHandlers.CommandHandlers;
using ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

namespace ServerApplication.Client.DataHandlers;

public class NurseHandler : DataHandler
{
    public NurseHandler(ClientData clientData) : base(clientData)
    {
        CommandHandler = new()
        {
            {"public-rsa-key", new RsaKey()},
            {"encryptedMessage", new EncryptedMessage(clientData.Server.Rsa)},
        };
    }
}