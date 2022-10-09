using ServerApplication.Client.DataHandlers.CommandHandlers;
using ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

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
                {"change-data", new ChangeData()},
                {"stop-bike-recording", new StopBikeRecording()}
            };
            
        }
    }
}