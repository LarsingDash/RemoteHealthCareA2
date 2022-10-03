using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Encryption;
using Shared.Log;

namespace Shared;

public class DefaultClientConnection
{
    #region ClientConnection
    private TcpClient client;
    private NetworkStream stream;
    private Dictionary<string, Action<JObject>> serialCallbacks = new();
    private Action<JObject, bool> commandHandlerMethod;
    
    public RSA Rsa = new RSACryptoServiceProvider();
    #endregion
    
    

    public DefaultClientConnection(string hostname, int port, Action<JObject, bool> commandHandlerMethod)
    {
        init(hostname, port, commandHandlerMethod);
        
    }

    public DefaultClientConnection()
    {
        
    }

    public void init(string hostname, int port, Action<JObject, bool> commandHandlerMethod)
    {
        OnMessage += (_, json) => HandleMessage(json);

        this.commandHandlerMethod = commandHandlerMethod;
        
        client = new(hostname, port);
        stream = client.GetStream();
        stream.BeginRead(_buffer, 0, 1024, OnRead, null);

        SetupClient();
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
        }, JsonFolderShared.Json.Path));
    }

    #region Sending and retrieving data
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
    /// <param name="json">The message that was sent from the client.</param>
    /// <param name="encrypted">If the messages was encrypted</param>
    /// <returns>
    /// The return value is a string.
    /// </returns>
    public void HandleMessage(JObject json, bool encrypted = false)
    {
        string extraText = encrypted ? "Encrypted " : "";
        if (!json.ContainsKey("id"))
        {
            Logger.LogMessage(LogImportance.Warn, $"Got {extraText}message with no id from server: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
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
        commandHandlerMethod.Invoke(json, encrypted);
        return;
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
    
    private static byte[] Concat(byte[] b1, byte[] b2, int count)
    {
        var r = new byte[b1.Length + count];
        Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
        Buffer.BlockCopy(b2, 0, r, b1.Length, count);
        return r;
    }
    
    public byte[] GetRsaPublicKey()
    {
        return Rsa.ExportRSAPublicKey();
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
        RSA newRsa = new RSACryptoServiceProvider();
        newRsa.ImportRSAPublicKey(PublicKey, out int a);

        var keyCrypt = RsaHelper.EncryptMessage(aes.Key, newRsa.ExportParameters(false), false);
        var iVCrypt = RsaHelper.EncryptMessage(aes.IV, newRsa.ExportParameters(false), false);
        var aesCrypt = AesHelper.EncryptMessage(message, aes.Key, aes.IV);

        if (keyCrypt != null && iVCrypt != null)
        {
            SendData(JsonFileReader.GetObjectAsString("EncryptedMessage", new Dictionary<string, string>()
            {
                {"\"_IV_\"", Util.ByteArrayToString(iVCrypt)},
                {"\"_key_\"", Util.ByteArrayToString(keyCrypt)},
                {"\"_data_\"", Util.ByteArrayToString(aesCrypt)},
            }, JsonFolderShared.Json.Path));
        }
        else
        {
            Logger.LogMessage(LogImportance.Warn, $"Could not send EncryptedData, something went wrong with encrypting the aes-keys");
        }
    }
    #endregion

    public void Disconnect()
    {
        stream.Close();
        client.Close();
    }
}