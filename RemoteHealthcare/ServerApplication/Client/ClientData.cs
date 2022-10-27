using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerApplication.Client.DataHandlers;
using Shared.Encryption;
using Shared.Log;
using Shared;
using ServerApplication.UtilData;
using JsonFolder = ServerApplication.UtilData.JsonFolder;

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
        private string? PublicKey { set; get; }
        #endregion

        private Dictionary<string, string> infoData = new Dictionary<string, string>();

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
                Thread.Sleep(500);
                var serialCallback = Util.RandomString();
                AddSerialCallback(serialCallback, json =>
                {
                    PublicKey = json["data"]!["key"]!.ToObject<string>();
                    Logger.LogMessage(LogImportance.Information, 
                        $"Received PublicKey from {UserName}: {LogColor.Gray}\n{PublicKey}");
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
                    Logger.LogMessage(LogImportance.Warn, $"Disconnected user {UserName}", ex);
                    if (DataHandler.GetType() == typeof(ClientHandler))
                    {
                        string text = JsonFileReader.GetObjectAsString("UserStateChange", new Dictionary<string, string>()
                        {
                            {"_username_", UserName},
                            {"_type_", "logout"}
                        }, JsonFolder.ClientMessages.Path);
                        foreach (var client in Server.users)
                        {
                            if (client.DataHandler.GetType() == typeof(DoctorHandler))
                            {
                                client.SendEncryptedData(text);
                            }
                        }
                    }
                    Server.RemoveUser(this);
                    return;
                }
        
                while (_totalBuffer.Length >= 4)
                {
                    var packetSize = BitConverter.ToInt32(_totalBuffer, 0);
        
                    if (_totalBuffer.Length >= packetSize + 4)
                    {
                        var json = Encoding.UTF8.GetString(_totalBuffer, 4, packetSize);
                        try
                        {
                            DataHandler.HandleMessage(this, JObject.Parse(json));
                        }
                        catch (IOException e)
                        {
                            Logger.LogMessage(LogImportance.Error, $"Could not parse json to JObject {LogColor.Gray}\n{json}", e);
                        }
        
                        var newBuffer = new byte[_totalBuffer.Length - packetSize - 4];
                        Array.Copy(_totalBuffer, packetSize + 4, newBuffer, 0, newBuffer.Length);
                        _totalBuffer = newBuffer;
                    }
                    else
                        break;
                }
        
                Stream.BeginRead(_buffer, 0, 1024, OnRead, null);
            }
            
            /// <summary>
            /// It takes two byte arrays and a count, and returns a new byte array that is the concatenation of the first
            /// two arrays, with the second array truncated to the count
            /// </summary>
            /// <param name="b1">The first byte array to concatenate.</param>
            /// <param name="b2">The buffer to copy from</param>
            /// <param name="count">The number of bytes to copy from the second array.</param>
            /// <returns>
            /// The byte array r is being returned.
            /// </returns>
            private static byte[] Concat(byte[] b1, byte[] b2, int count)
            {
                var r = new byte[b1.Length + count];
                Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
                Buffer.BlockCopy(b2, 0, r, b1.Length, count);
                return r;
            }
            
            private Queue<Tuple<string, bool>> sendQueue = new();
            private Queue<Tuple<string, bool>> sendQueuePrio = new();
    

            private bool sending = false;
            private void Send()
            {
                var t = new Thread(start =>
                {
                    if (sending)
                        return;
                    sending = true;
                    while (sendQueue.Count > 0 || sendQueuePrio.Count > 0)
                    {
                        //new Thread(start => Logger.LogMessage(LogImportance.Error, sendQueue.Count.ToString())).Start();
                        try
                        {
                            if (sendQueuePrio.Count > 0)
                            {
                                Tuple<string, bool> val = sendQueuePrio.Dequeue();
                                SendMessage(val.Item1, val.Item2);
                            }
                            else
                            {
                                Tuple<string, bool> val = sendQueue.Dequeue();
                                SendMessage(val.Item1, val.Item2);
                            }
                            Thread.Sleep(2);
                        }
                        catch (Exception e)
                        {
                            Logger.LogMessage(LogImportance.Error, "Could not send message", e);
                        }
                    }
                    sending = false;
                });
                t.IsBackground = true;
                t.Start();
            }
            
            /// <summary>
            /// It takes a string, converts it to a byte array, and sends it to the server
            /// </summary>
            /// <param name="message">The string to send to the server, needs to be a json</param>
            public void SendData(string message, bool hide = false, bool priority = false)
            {
                if (priority)
                {
                    sendQueuePrio.Enqueue(Tuple.Create(message, hide));
                }
                else
                {
                    sendQueue.Enqueue(Tuple.Create(message, hide));
                }
                Send();
        
            }
            
            private void SendMessage(string message, bool hide)
            {
                var t = new Thread(start =>
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

                        if (!ob["id"]!.ToObject<string>()!.Equals("encryptedMessage") && !hide)
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
                });
                t.Start();
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

                if (PublicKey == null || PublicKey.Length < 3)
                {
                    Logger.LogMessage(LogImportance.Error, "Could not send encrypted data, no PublicKey UserName: " + UserName);
                    return;
                }
                Aes aes = Aes.Create("AesManaged")!;
                RSA rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(PublicKey!);

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

            public string GetInfo(string infoId)
            {
                return infoData.ContainsKey(infoId) ? infoData[infoId] : "_NotFound_";
            }

            public void AddInfo(string infoId, string value)
            {
                if (infoData.ContainsKey(infoId))
                {
                    infoData.Remove(infoId);
                }
                infoData.Add(infoId, value);
            }
    }
    
    
}