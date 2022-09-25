using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerApplication.Client.DataHandlers;
using ServerApplication.Encryption;
using ServerApplication.Log;
using ServerApplication.UtilData;

namespace ServerApplication.Client
{
    public class ClientData
    {
        #region References
        public Server Server { get; }
        #endregion
        
        #region Sending and retrieving data

        private TcpClient Client { get; }
        private NetworkStream Stream { get; }
        public Dictionary<string, Action<JObject>> SerialCallbacks = new();
        public DataHandler DataHandler;
        #endregion
        
        #region Userdata
        public string UserName { get; set; }
        private byte[]? PublicKey { set; get; }
        #endregion


        public ClientData(Server server, TcpClient client)
        {
            this.Server = server;
            Client = client;
            DataHandler = new DefaultHandler(this);
            Stream = Client.GetStream();
            Stream.BeginRead(_buffer, 0, 1024, OnRead, null);
            
            UserName = "Unknown";
            new Task((() =>
            {
                var serialCallback = Util.RandomString();
                AddSerialCallback(serialCallback, json =>
                {
                    PublicKey = json["data"]?.Value<JArray>("key")?.Values<byte>().ToArray() ?? Array.Empty<byte>();
                    Logger.LogMessage(LogImportance.Information, 
                        $"Received PublicKey from {UserName}: {LogColor.Gray}\n{Util.ByteArrayToString(PublicKey)}");
                });
                SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", new Dictionary<string,string>()
                {
                    {"_serial_", serialCallback}
                }, JsonFolder.ClientMessages.Path));
            })).Start();
        }

        #region Sending and retrieving data.
        
            private byte[] _totalBuffer = Array.Empty<byte>();
            private readonly byte[] _buffer = new byte[1024];
            /// <summary>
            /// We read the stream, and if we have enough data to read a packet, we read it, and then we read the next
            /// packet
            /// </summary>
            /// <param name="readResult">The result of the asynchronous operation.</param>
            private void OnRead(IAsyncResult readResult)
            {
                try
                {
                    var numberOfBytes = Stream.EndRead(readResult);
                    _totalBuffer = Concat(_totalBuffer, _buffer, numberOfBytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex}");
                    return;
                }
        
                while (_totalBuffer.Length >= 4)
                {
                    var packetSize = BitConverter.ToInt32(_totalBuffer, 0);
        
                    if (_totalBuffer.Length >= packetSize + 4)
                    {
                        var json = Encoding.UTF8.GetString(_totalBuffer, 4, packetSize);
                        DataHandler.HandleMessage(this, JObject.Parse(json));
        
                        var newBuffer = new byte[_totalBuffer.Length - packetSize - 4];
                        Array.Copy(_totalBuffer, packetSize + 4, newBuffer, 0, newBuffer.Length);
                        _totalBuffer = newBuffer;
                    }
                    else
                        break;
                }
        
                Stream.BeginRead(_buffer, 0, 1024, OnRead, null);
            }
            
            private static byte[] Concat(byte[] b1, byte[] b2, int count)
            {
                var r = new byte[b1.Length + count];
                Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
                Buffer.BlockCopy(b2, 0, r, b1.Length, count);
                return r;
            }
            
            /// <summary>
            /// It takes a string, converts it to a byte array, and sends it to the server
            /// </summary>
            /// <param name="message">The string to send to the server, needs to be a json</param>
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
                    Logger.LogMessage(LogImportance.Information, 
                        $"Sending message: {LogColor.Gray}\n{ob.ToString(Formatting.None)}");
                }
                catch(JsonReaderException)
                {
                    Logger.LogMessage(LogImportance.Information, 
                        $"Sending message: {LogColor.Gray}\n(_NonJsonObject_)");
                }
                
                Byte[] data = BitConverter.GetBytes(message.Length);
                Byte[] comman = System.Text.Encoding.ASCII.GetBytes(message);
                Stream.Write(data, 0, data.Length);
                Stream.Write(comman, 0, comman.Length);
            }

            /// <summary>
            /// It encrypts the message with AES, encrypts the AES key and IV with RSA, and sends the encrypted message
            /// </summary>
            /// <param name="message">The message to be encrypted</param>
            public void SendEncryptedData(String message)
            {
                try
                {
                    Logger.LogMessage(LogImportance.Information, 
                        $"Sending encrypted message: {LogColor.Gray}\n{JObject.Parse(message).ToString(Formatting.None)}");
                }
                catch (JsonReaderException)
                {
                    Logger.LogMessage(LogImportance.Information,
                        $"Sending encrypted message: {LogColor.Gray}\n(_NonJsonObject_)");
                }
                Aes aes = Aes.Create("AesManaged")!;
                RSA rsa = new RSACryptoServiceProvider();
                rsa.ImportRSAPublicKey(PublicKey, out int a);

                var keyCrypt = RsaHelper.EncryptMessage(aes.Key, rsa.ExportParameters(false), false);
                var iVCrypt = RsaHelper.EncryptMessage(aes.IV, rsa.ExportParameters(false), false);
                var aesCrypt = AesHelper.EncryptMessage(message, aes.Key, aes.IV);

                if (keyCrypt != null && iVCrypt != null)
                {
                    SendData(JsonFileReader.GetObjectAsString("EncryptedMessage", new Dictionary<string, string>()
                    {
                        {"\"_IV_\"", Util.ByteArrayToString(iVCrypt)},
                        {"\"_key_\"", Util.ByteArrayToString(keyCrypt)},
                        {"\"_data_\"", Util.ByteArrayToString(aesCrypt)},
                    }, JsonFolder.Json.Path));
                }
                else
                {
                    Logger.LogMessage(LogImportance.Warn, $"Could not send EncryptedData, something went wrong with encrypting the aes-keys");
                }
                

            }
            #endregion
            
            /// <summary>
            /// > This function adds a callback to the SerialCallbacks dictionary
            /// </summary>
            /// <param name="serial">The serial number of the device you want to listen for.</param>
            /// <param name="action">The action to be called when the response is received.</param>
            public void AddSerialCallback(string serial, Action<JObject> action)
            {
                if (SerialCallbacks.ContainsKey(serial))
                {
                    SerialCallbacks.Remove(serial);
                }
        
                SerialCallbacks.Add(serial, action);
            }
    }
    
    
}