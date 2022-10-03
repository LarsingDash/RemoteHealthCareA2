using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using DoctorApplication.Communication.CommandHandlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerApplication;
using ServerApplication.Encryption;
using ServerApplication.Log;
using Formatting = Newtonsoft.Json.Formatting;

namespace DoctorApplication.Communication;

public class Client
{
    #region ClientConnection
    private TcpClient client;
    private NetworkStream stream;
    private Dictionary<string, Action<JObject>> serialCallbacks = new();
    private Dictionary<string, ICommandHandler> commandHandler;
    #endregion
    
    

    public Client()
    { 
        commandHandler = new Dictionary<string, ICommandHandler>()
        {
            {"public-rsa-key", new RsaKey()},
            {"encryptedMessage", new EncryptedMessage(rsa)}
        };
        OnMessage += async (_, json) => await HandleMessage(json);
        
        client = new("127.0.0.1", 2460);
        stream = client.GetStream();
        stream.BeginRead(_buffer, 0, 1024, OnRead, null);

        SetupClient();
        
        Thread.Sleep(500);
        
        //Testing...
        var serial = Util.RandomString();
        SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
        {
            {"_serial_", serial},
            {"_type_", "Doctor"},
            {"_username_", "Jasper"},
            {"_password_", "Merijn"}
        }, JsonFolder.Json.Path));
        
        AddSerialCallback(serial, ob =>
        {
           // Logger.LogMessage();
        });
        
        SendEncryptedData(JsonFileReader.GetObjectAsString("ActiveClients", new Dictionary<string, string>()
        {
            //{"_serial_", serial}
        }, JsonFolder.Json.Path));
        
        Thread.Sleep(500);
        SendEncryptedData(JsonFileReader.GetObjectAsString("HistoricClientData", new Dictionary<string, string>()
        {
            {"_name_", "Tes123t1"}
        }, JsonFolder.Json.Path));
        
        SendEncryptedData(JsonFileReader.GetObjectAsString("AllClients", new Dictionary<string, string>(), JsonFolder.Json.Path));
        

    }

    private void SetupClient()
    {
        var serial = Util.RandomString();
        AddSerialCallback(serial, ob =>
        {
            PublicKey = ob["data"].Value<JArray>("key").Values<byte>().ToArray();
        });
        
        SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", new Dictionary<string, string>()
        {
            {"_serial_", serial}
        }, JsonFolder.Json.Path));
        
    }

    #region Sending and retrieving data
    private RSA rsa = new RSACryptoServiceProvider();
    private byte[] _totalBuffer = Array.Empty<byte>();
    private readonly byte[] _buffer = new byte[1024];
    public event EventHandler<JObject> OnMessage;
    private byte[] PublicKey;
    /// <summary>
    /// It checks if the message has a serial, if it does it checks if the client has a callback for that serial, if it does
    /// it calls the callback and removes it from the list, if it doesn't it checks if the message has an id, if it does it
    /// checks if the server has a command handler for that id, if it does it calls the command handler, if it doesn't it
    /// logs a warning
    /// </summary>
    /// <param name="ClientData">The clientData object of the client that sent the message.</param>
    /// <param name="JObject">The message that was sent from the client.</param>
    /// <returns>
    /// The return value is a string.
    /// </returns>
        public async Task HandleMessage(JObject json)
        {
            if (!json.ContainsKey("id"))
            {
                Logger.LogMessage(LogImportance.Warn, $"Got message with no id from server: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
                return;
            }
            if (json.ContainsKey("serial"))
            {
                var serial = json["serial"]!.ToObject<string>();
                if (serialCallbacks.ContainsKey(serial!))
                {
                    serialCallbacks[serial!].Invoke(json);
                    serialCallbacks.Remove(serial!);
                    return;
                }
            }

            if (commandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
            {
                if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
                {
                    Logger.LogMessage(LogImportance.Information, $"Got message from server: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
                }
                commandHandler[json["id"]!.ToObject<string>()!].HandleCommand(this, json);
            }
            else
            {
                Logger.LogMessage(LogImportance.Warn, $"Got message from server but no commandHandler found: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
            }
        }
    
    private void OnRead(IAsyncResult readResult)
    {
        try
        {
            var numberOfBytes = stream.EndRead(readResult);
            _totalBuffer = Concat(_totalBuffer, _buffer, numberOfBytes);
        }
        catch
        {
            return;
        }

        while (_totalBuffer.Length >= 4)
        {
            var packetSize = BitConverter.ToInt32(_totalBuffer, 0);

            if (_totalBuffer.Length >= packetSize + 4) 
            {
                var json = Encoding.UTF8.GetString(_totalBuffer, 4, packetSize);

                OnMessage?.Invoke(this, JObject.Parse(json));

                var newBuffer = new byte[_totalBuffer.Length - packetSize - 4];
                Array.Copy(_totalBuffer, packetSize + 4, newBuffer, 0, newBuffer.Length);
                _totalBuffer = newBuffer;
            }

            else
                break;
        }

        stream.BeginRead(_buffer, 0, 1024, OnRead, null);
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
        stream.Write(data, 0, data.Length);
        stream.Write(comman, 0, comman.Length);
    }
    
    private static byte[] Concat(byte[] b1, byte[] b2, int count)
    {
        var r = new byte[b1.Length + count];
        Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
        Buffer.BlockCopy(b2, 0, r, b1.Length, count);
        return r;
    }
    
    public byte[] GetRsaPublicKey()
    {
        return rsa.ExportRSAPublicKey();
    }
    
    public void AddSerialCallback(string serial, Action<JObject> action)
    {
        if (serialCallbacks.ContainsKey(serial))
        {
            serialCallbacks.Remove(serial);
        }
        
        serialCallbacks.Add(serial, action);
    }
    
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
}