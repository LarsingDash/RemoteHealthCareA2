using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClientSide.Encryption;
using ClientSide.Helpers;
using ClientSide.VR;
using Newtonsoft.Json.Linq;
using ServerApplication;
using Util = ClientSide.Helpers.Util;

namespace ClientSide
{
    internal class Client
    {
        private static TcpClient client;
        private static NetworkStream stream;
        private string userID;

        private static byte[] totalBuffer = Array.Empty<byte>();
        private static readonly byte[] buffer = new byte[1024];

        private RSA rsa = new RSACryptoServiceProvider();
        private byte[] PublicKey;

        private Dictionary<string, Action<JObject>> serialCallbacks = new();
        
        public Client()
        {
            
            OnMessage += async (_, json) => await ProcessMessageAsync(json);
            
            try
            {
                Console.WriteLine("Connecting to server...");
                client = new TcpClient();
                client.BeginConnect("localhost", 2460, new AsyncCallback(OnConnect), null);

                //TODO: Replace with login screen after login is implemented
                
                while (true)
                {
                    Console.WriteLine("Send command:");
                    var command = Console.ReadLine();

                    switch (command)
                    {
                        case "login":
                            SendData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
                            {
                                {"_type_", "Client"},
                                {"_username_", "TestUsername"},
                                {"_serial_", "TestSerial"},
                                {"_password_", "TestPassword"}
                            }, JsonFolder.Json.path));
                            break;
                        
                        case "start bike recording":
                            SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecording", new Dictionary<string, string>()
                            {
                                {"_name_", "TestName"},
                                {"_serial_", "TestSerial"},
                            }, JsonFolder.Json.path));
                            break;
                        
                        case "stop bike recording":
                            SendEncryptedData(JsonFileReader.GetObjectAsString("StopBikeRecording", new Dictionary<string, string>()
                            {
                                {"_uuid_", "TestUuid"},
                                {"_serial_", "TestSerial"},
                            }, JsonFolder.Json.path));
                            break;
                        
                        case "exit":
                            CloseConnection();
                            break;
                    }
                }
            }
            catch(Exception exception)
            {
                Console.WriteLine($"Could not connect with server... {exception}");
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            client.EndConnect(ar);
            Console.WriteLine("Connected to server!");
            stream = client.GetStream();
            stream.BeginRead(buffer, 0, 1024, OnRead, null);
        }

        private static void CloseConnection()
        {
            stream.Close();
            client.Close();
        }

        private void OnRead(IAsyncResult readResult)
        {
            try
            {
                var numberOfBytes = stream.EndRead(readResult);
                totalBuffer = Concat(totalBuffer, buffer, numberOfBytes);
            }
            catch
            {
                return;
            }

            while (totalBuffer.Length >= 4)
            {
                var packetSize = BitConverter.ToInt32(totalBuffer, 0);

                if (totalBuffer.Length >= packetSize + 4) 
                {
                    var json = Encoding.UTF8.GetString(totalBuffer, 4, packetSize);
                    OnMessage?.Invoke(this, JObject.Parse(json));

                    var newBuffer = new byte[totalBuffer.Length - packetSize - 4];
                    Array.Copy(totalBuffer, packetSize + 4, newBuffer, 0, newBuffer.Length);
                    totalBuffer = newBuffer;
                }

                else
                    break;
            }

            stream.BeginRead(buffer, 0, 1024, OnRead, null);
        }
        
        private void SendData(String s)
        {
            Console.WriteLine($"Sending data to server: {s}");
            Byte[] data = BitConverter.GetBytes(s.Length);
            Byte[] comman = System.Text.Encoding.ASCII.GetBytes(s);
            stream.Write(data, 0, data.Length);
            stream.Write(comman, 0, comman.Length);
        }
        public void SendEncryptedData(String message)
        {
            Console.WriteLine("Send Encrypted Message...");
            Aes aes = Aes.Create("AesManaged") ?? new AesManaged();
            rsa.ImportRSAPublicKey(PublicKey, out int a);

            var keyCrypt = ClientSide.Encryption.RSAHelper.EncryptMessage(aes.Key, rsa.ExportParameters(false), false);
            var IVCrypt = ClientSide.Encryption.RSAHelper.EncryptMessage(aes.IV, rsa.ExportParameters(false), false);
            var aesCrypt = ClientSide.Encryption.AESHelper.EncryptMessage(message, aes.Key, aes.IV);

            var serial = Util.RandomString();
                
            AddSerialCallback(serial, ob =>
            {
                Console.WriteLine("Ping: " + ob);
                var keyCrypted = ob["aes-keys"].Value<JArray>("Key").Values<byte>().ToArray();
                var iVCrypted = ob["aes-keys"].Value<JArray>("IV").Values<byte>().ToArray();
                    
                var messageCrypted = ob.Value<JArray>("aes-data").Values<byte>().ToArray();  
                    
                var key = ClientSide.Encryption.RSAHelper.DecryptMessage(keyCrypted, rsa.ExportParameters(true), false);
                var iV = ClientSide.Encryption.RSAHelper.DecryptMessage(iVCrypted, rsa.ExportParameters(true), false);

                var message = ClientSide.Encryption.AESHelper.DecryptMessage(messageCrypted, key, iV);
            });

            SendData(JsonFileReader.GetObjectAsString("EncryptedMessage", new Dictionary<string, string>()
            {
                {"\"_IV_\"", Util.ByteArrayToString(IVCrypt)},
                {"\"_key_\"", Util.ByteArrayToString(keyCrypt)},
                {"\"_data_\"", Util.ByteArrayToString(aesCrypt)},
                {"_serial_", serial}
            }, JsonFolder.Json.path));

        }
        
        public void AddSerialCallback(string serial, Action<JObject> action)
        {
            if (serialCallbacks.ContainsKey(serial))
            {
                serialCallbacks.Remove(serial);
            }
        
            serialCallbacks.Add(serial, action);
        }
        
        public byte[] GetRsaPublicKey()
        {
            return rsa.ExportRSAPublicKey();
        }
        
        private static byte[] Concat(byte[] b1, byte[] b2, int count)
        {
            var r = new byte[b1.Length + count];
            Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
            Buffer.BlockCopy(b2, 0, r, b1.Length, count);
            return r;
        }
        
        public static event EventHandler<JObject> OnMessage;
        
        private async Task ProcessMessageAsync(JObject json)
        {
            Console.WriteLine($"Received: {json}");
            if (json.ContainsKey("serial"))
            {
                if (json["serial"].ToObject<string>().Equals("ping"))
                {
                    SendData(json.ToString());
                    return;
                }
                var serial = json["serial"].ToObject<string>();
                if (serialCallbacks.ContainsKey(serial))
                {
                    serialCallbacks[serial].Invoke(json);
                    serialCallbacks.Remove(serial);
                    return;
                }
            }
            switch (json["id"].ToObject<string>())
            {
                case "public-rsa-key":
                {
                    SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", new Dictionary<string,string>()
                    {
                        {"\"_key_\"", Util.ByteArrayToString(GetRsaPublicKey())},
                        {"_serial_", json["serial"].ToObject<string>()}
                    }, JsonFolder.Json.path));
                    break;
                }
                case "encryptedMessage":
                {
                    Console.WriteLine("Encrypted Message");
                    var keyCrypted = json["aes-keys"].Value<JArray>("Key").Values<byte>().ToArray();
                    var iVCrypted = json["aes-keys"].Value<JArray>("IV").Values<byte>().ToArray();
                    
                    var messageCrypted = json.Value<JArray>("aes-data").Values<byte>().ToArray();
                    
                    var key = RSAHelper.DecryptMessage(keyCrypted, rsa.ExportParameters(true), false);
                    var iV = RSAHelper.DecryptMessage(iVCrypted, rsa.ExportParameters(true), false);
                
                    var message = AESHelper.DecryptMessage(messageCrypted, key, iV);

                    Console.WriteLine($"Received Message: {message}");
                    break;
                }
            }
        }
    }
}
