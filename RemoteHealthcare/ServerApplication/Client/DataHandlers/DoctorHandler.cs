using ServerApplication.Client.DataHandlers.CommandHandlers;
using ServerApplication.Client.DataHandlers.CommandHandlers.Doctor;

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
                {"active-clients", new ActiveClients()},
                {"historic-client-data", new HistoricClientData()},
                {"all-clients", new AllClients()},
                {"start-bike-recording", new StartBikeRecording()}
            };
        }
    }
}