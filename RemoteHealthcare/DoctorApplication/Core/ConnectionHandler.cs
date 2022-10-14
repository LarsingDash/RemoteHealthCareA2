using Shared;
using Shared.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using ClientApplication.ServerConnection;
using ClientApplication.ServerConnection.Communication;

namespace DoctorApplication.Core
{
    internal class ConnectionHandler
    {
        private Client client { get; set; }
        private string serial { get; set; }
        private string? uuid { get; set; }

        public ConnectionHandler()
        {
            this.client = App.GetClientInstance();
            this.serial = Util.RandomString();
        }

        public void ListClients()
        {
            client.SendEncryptedData(JsonFileReader.GetObjectAsString("AllClients", new Dictionary<string, string>()
            {
                {"_serial_", serial}
            }, JsonFolder.Json.Path));
            client.AddSerialCallback(serial, ob =>
            {
                Logger.LogMessage(LogImportance.Fatal, ob.ToString());

            });
        }

        public async Task SubscribeToSessionAsync()
        {
            serial = Util.RandomString();
            client.SendEncryptedData(JsonFileReader.GetObjectAsString("SubscribeToSession", new Dictionary<string, string>()
        {
            {"_uuid_", uuid},
            {"_serial_", serial}
        }));
            await client.AddSerialCallbackTimeout(serial, ob =>
            {
                //Check status ok
            }, () =>
            {
            }, 1000);
        }

        public async Task StartRecordingAsync(string patientUserName)
        {
            var serial = Util.RandomString();
            client.SendData(JsonFileReader.GetObjectAsString("StartBikeRecording", new Dictionary<string, string>()
        {
            {"_session_", "TestSession1"},
            {"_serial_", serial},
            {"_name_", patientUserName}
        }, JsonFolder.Json.Path));
            await client.AddSerialCallbackTimeout(serial, ob =>
            {
                if (ob["data"]!["status"]!.ToObject<string>()!.Equals("ok"))
                {
                    uuid = ob["data"]!["uuid"]!.ToObject<string>()!;
                }
                else
                {
                    Logger.LogMessage(LogImportance.Error ,"Could not start Bike recording. Error: " + ob["data"]?["error"]?.ToObject<string>());
                }
            }, () =>
            {
                Logger.LogMessage(LogImportance.Error, "Did not get a response from start-bike-recording");
            }, 1000);
        }
    }
}
