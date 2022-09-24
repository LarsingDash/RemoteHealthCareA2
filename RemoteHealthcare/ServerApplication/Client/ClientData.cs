using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientSide.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerSide;
using SharedProject;
using SharedProject.Log;

namespace ServerApplication
{
    public class ClientData
    {
        private Server server { get; }
        public TcpClient Client { get; }
        public NetworkStream Stream { get; }
        public string UserName { get; }
        public byte[]? PublicKey { set; private get; }

        private Random random;
        private UnicodeEncoding byteConverter;


        public ClientData(Server server, TcpClient client)
        {
            Client = client;
            this.server = server;
            Stream = Client.GetStream();
            UserName = "Unknown";

            this.random = new Random();
            this.byteConverter = new UnicodeEncoding();
            
            Stream.BeginRead(_buffer, 0, 1024, OnRead, null);
            
            new Task((() =>
            {
                var serialCallback = Util.RandomString();
                server.AddSerialCallback(serialCallback, json =>
                {
                    PublicKey = json["data"]?.Value<JArray>("key")?.Values<byte>().ToArray() ?? Array.Empty<byte>();
                    Logger.LogMessage(LogImportance.Information, 
                        $"Received PublicKey from {UserName}: {LogColor.Gray}\n{Util.ByteArrayToString(PublicKey)}");
                });
                Thread.Sleep(1000);
                SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", new Dictionary<string,string>()
                {
                    {"_serial_", serialCallback}
                }, JsonFolder.ClientMessages.path));
            })).Start();
        }

        #region Sending and retrieving data.
        
            private byte[] _totalBuffer = Array.Empty<byte>();
            private readonly byte[] _buffer = new byte[1024];
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
                        server.OnMessage(this, JObject.Parse(json));
        
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
            /// <param name="String">The string to send to the server, needs to be a json</param>
            public void SendData(String s)
            {
                try
                {
                    JObject ob = JObject.Parse(s);
                    if (ob.ContainsKey("serial"))
                    {
                        if (ob["serial"]!.ToObject<string>()!.Equals("_serial_"))
                        {
                            ob.Remove("serial");
                            s = ob.ToString();
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

                //Console.WriteLine($"Sending data to {UserName}: {s}");
                Byte[] data = BitConverter.GetBytes(s.Length);
                Byte[] comman = System.Text.Encoding.ASCII.GetBytes(s);
                Stream.Write(data, 0, data.Length);
                Stream.Write(comman, 0, comman.Length);
            }

            public void SendEncryptedData(String message)
            {
                try
                {
                    Logger.LogMessage(LogImportance.Information, 
                        $"Sending encrypted message: {LogColor.Gray}\n{JObject.Parse(message).ToString(Formatting.None)}");
                }
                catch (JsonReaderException e)
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
                    }, JsonFolder.Json.path));
                }
                else
                {
                    Logger.LogMessage(LogImportance.Error, $"Could not send EncryptedData, something went wrong with encrypting the aes-keys");
                }
                

            }
            #endregion
    }
}