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
                {"change-data", new ChangeData()},
                {"start-bike-recording", new StartBikeRecording()},
                {"stop-bike-recording", new StopBikeRecording()}
            };
            
        }
    }
}