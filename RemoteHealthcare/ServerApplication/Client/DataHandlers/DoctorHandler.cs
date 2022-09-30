using ServerApplication.Client.DataHandlers.CommandHandlers;

namespace ServerApplication.Client.DataHandlers
{
    public class DoctorHandler : DataHandler
    {
        public DoctorHandler(ClientData clientData) : base(clientData)
        {
            CommandHandler = new()
            {
                {"public-rsa-key", new RsaKey()},
                {"encryptedMessage", new EncryptedMessage(clientData.Server.Rsa)},
            };
        }
    }
}