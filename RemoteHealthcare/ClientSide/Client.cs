using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using ClientApplication.ServerConnection.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Encryption;
using Shared.Log;
using JsonFolder = ClientApplication.ServerConnection.Helpers.JsonFolder;

namespace ClientApplication.ServerConnection
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
                client = new("localhost", 2460);
                stream = client.GetStream();
                stream.BeginRead(buffer, 0, 1024, OnRead, null);
                
                var serial = Util.RandomString();
                SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", new Dictionary<string, string>()
                {
                    {"_serial_", serial}
                }, JsonFolder.Json.Path));
                AddSerialCallback(serial, ob =>
                {
                    PublicKey = ob["data"]?.Value<JArray>("key")?.Values<byte>().ToArray() ?? Array.Empty<byte>();
                    Logger.LogMessage(LogImportance.Information, 
                        $"Received PublicKey: {LogColor.Gray}\n{Util.ByteArrayToString(PublicKey)}");
                });

                //TODO: Replace with login screen after login is implemented
                
                while (true)
                {
                    Console.WriteLine("Send command:");
                    var command = Console.ReadLine();

                    switch (command)
                    {
                        case "login":
                            SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
                            {
                                {"_type_", "Client"},
                                {"_username_", "TestUsername"},
                                {"_serial_", "TestSerial"},
                                {"_password_", "TestPassword"}
                            }, JsonFolder.Json.Path));
                            break;
                        
                        case "start bike recording":
                            SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecording", new Dictionary<string, string>()
                            {
                                {"_name_", "TestName"},
                                {"_serial_", "TestSerial"},
                            }, JsonFolder.Json.Path));
                            break;
                        
                        case "stop bike recording":
                            SendEncryptedData(JsonFileReader.GetObjectAsString("StopBikeRecording", new Dictionary<string, string>()
                            {
                                {"_uuid_", "TestUuid"},
                                {"_serial_", "TestSerial"},
                            }, JsonFolder.Json.Path));
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
        
        public void SendData(string message)
        {
            try
            {
                var ob = JObject.Parse(message);
                if (ob.ContainsKey("serial"))
                {
                    if (ob["serial"]!.ToObject<string>()!.Equals("_serial_"))
                    {
                        ob.Remove("serial");
                        message = ob.ToString();
                    }
                }

                if (ob["data"]?["error"]?.ToObject<string>() != null)
                {
                    if (ob["data"]!["error"]!.ToObject<string>()!.Equals("_error_"))
                    {
                        ob["data"]!["error"]!.Remove();
                        message = ob.ToString();
                    }
                }

                if (!ob["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
                {
                    Logger.LogMessage(LogImportance.Information, 
                        $"Sending message: {LogColor.Gray}\n{ob.ToString(Formatting.None)}");
                }
            }
            catch(JsonReaderException)
            {
                Logger.LogMessage(LogImportance.Information, 
                    $"Sending message: {LogColor.Gray}\n(_NonJsonObject_)");
            }
                
            Byte[] data = BitConverter.GetBytes(message.Length);
            Byte[] comman = System.Text.Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
            stream.Write(comman, 0, comman.Length);
        }
        public void SendEncryptedData(String message)
        {
            Console.WriteLine("Send Encrypted Message...");
            Aes aes = Aes.Create("AesManaged") ?? new AesManaged();
            RSA cRsa = new RSACryptoServiceProvider();
            cRsa.ImportRSAPublicKey(PublicKey, out int a);

            var keyCrypt = RsaHelper.EncryptMessage(aes.Key, cRsa.ExportParameters(false), false);
            var IVCrypt = RsaHelper.EncryptMessage(aes.IV, cRsa.ExportParameters(false), false);
            var aesCrypt = AesHelper.EncryptMessage(message, aes.Key, aes.IV);

            var serial = Util.RandomString();
                
            AddSerialCallback(serial, ob =>
            {
                Console.WriteLine("Ping: " + ob);
                var keyCrypted = ob["aes-keys"].Value<JArray>("Key").Values<byte>().ToArray();
                var iVCrypted = ob["aes-keys"].Value<JArray>("IV").Values<byte>().ToArray();
                    
                var messageCrypted = ob.Value<JArray>("aes-data").Values<byte>().ToArray();  
                    
                var key = RsaHelper.DecryptMessage(keyCrypted, cRsa.ExportParameters(true), false);
                var iV = RsaHelper.DecryptMessage(iVCrypted, cRsa.ExportParameters(true), false);

                var message = AesHelper.DecryptMessage(messageCrypted, key, iV);
            });

            SendData(JsonFileReader.GetObjectAsString("EncryptedMessage", new Dictionary<string, string>()
            {
                {"\"_IV_\"", Util.ByteArrayToString(IVCrypt)},
                {"\"_key_\"", Util.ByteArrayToString(keyCrypt)},
                {"\"_data_\"", Util.ByteArrayToString(aesCrypt)},
                {"_serial_", serial}
            }, JsonFolder.Json.Path));

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
            Logger.LogMessage(LogImportance.Information, $"Got message: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
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
                    }, JsonFolder.Json.Path));
                    break;
                }
                case "encryptedMessage":
                {
                    try
                    {
                        var keyCrypted = json["aes-keys"]!.Value<JArray>("Key")!.Values<byte>().ToArray();
                        var iVCrypted = json["aes-keys"]!.Value<JArray>("IV")!.Values<byte>().ToArray();
                        
                        var messageCrypted = json.Value<JArray>("aes-data")!.Values<byte>().ToArray();  
                        
                        var key = RsaHelper.DecryptMessage(keyCrypted, rsa.ExportParameters(true), false);
                        var iV = RsaHelper.DecryptMessage(iVCrypted, rsa.ExportParameters(true), false);
                        if (key != null && iV != null)
                        {
                            var message = AesHelper.DecryptMessage(messageCrypted, key, iV);
                            if (message != null)
                            {
                                try
                                {
                                    JObject newJson = JObject.Parse(message);
                                    //Logger.LogMessage(LogImportance.Information, $"Got encrypted message: {LogColor.Gray}\n{ob.ToString(Formatting.None)}");
                                    ProcessMessageAsync(newJson);
                                } catch(JsonReaderException e)
                                {
                                    Logger.LogMessage(LogImportance.Warn, $"Got encrypted message, but message could not be parsed to JSON: {LogColor.Gray}\n{message}", e);
                                }
                            }
                            else
                            {
                                Logger.LogMessage(LogImportance.Warn, "Got encrypted message, but message is null");
                            }
                        }
                        else
                        {
                            Logger.LogMessage(LogImportance.Warn, "Received encrypted Message, but could not decrypt Aes-keys. Did sender use correct Public key of RSA");
                        }
                    }
                    catch(Exception ex)
                    {
                        Logger.LogMessage(LogImportance.Warn, "Error (Unknown Reason)", ex);
                    }

                    break;
                }
            }
        }
    }
}
