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
    public readonly Dictionary<string, Action<JObject>> SerialCallbacks = new();
    private Action<JObject, bool> commandHandlerMethod;
    
    public RSA Rsa = new RSACryptoServiceProvider();
    #endregion
    
    public bool Connected = false;
    public DefaultClientConnection(string hostname, int port, Action<JObject, bool> commandHandlerMethod)
    {
        Init(hostname, port, commandHandlerMethod);

    }
    [Obsolete("Only use this when you call init manually")]
    public DefaultClientConnection()
    {
        
    }

    /// <summary>
    /// `Init` is a function that takes a hostname, port, and a command handler method, and sets up a client to connect to
    /// the hostname and port, and sets up a stream to read from the client, and sets up a buffer to read into, and sets up
    /// a function to handle messages from the server
    /// </summary>
    /// <param name="hostname">The hostname of the server you want to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    /// <param name="commandHandlerMethod">This is the method that will be called when a command is received from the
    /// server.</param>
    public void Init(string hostname, int port, Action<JObject, bool> commandHandlerMethod, bool setup = true)
    {
        OnMessage += (_, json) => HandleMessage(json);

        this.commandHandlerMethod = commandHandlerMethod;
        try
        {
            client = new(hostname, port);
            stream = client.GetStream();
            stream.BeginRead(buffer, 0, 40960000, OnRead, null);
            SetupClient();
            Connected = true;
        }
        catch (SocketException e)
        {
            Logger.LogMessage(LogImportance.Fatal, "Could not connect with server", e);
        }
    }

    /// <summary>
    /// It sends a request to the server for the public key, and when the server responds, it sets the public key to the
    /// value it received
    /// </summary>
    public void SetupClient()
    {
        Thread.Sleep(100);
        var serial = Util.RandomString();
        AddSerialCallback(serial, ob =>
        {
            publicKey = ob["data"]!["key"]!.ToObject<string>()!;
            Logger.LogMessage(LogImportance.Information, 
                $"Received PublicKey from Server: {LogColor.Gray}\n{(publicKey)}");
        });
        
        SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", new Dictionary<string, string>()
        {
            {"_serial_", serial}
        }, JsonFolderShared.Json.Path));
    }

    #region Sending and retrieving data
    private byte[] totalBuffer = Array.Empty<byte>();
    private readonly byte[] buffer = new byte[40960000];
    public event EventHandler<JObject> OnMessage; 
    private string publicKey;

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
        // Logger.LogMessage(LogImportance.Debug, "Received " + json["id"].ToObject<string>());
        // Logger.LogMessage(LogImportance.DebugHighlight, json.ToString(Formatting.None));
        if (!json.ContainsKey("id"))
        {
            Logger.LogMessage(LogImportance.Warn, $"Got {extraText}message with no id from server: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
            return;
        }
        if (json.ContainsKey("serial"))
        {
            var serial = json["serial"]!.ToObject<string>();
            if (SerialCallbacks.ContainsKey(serial!))
            {
                SerialCallbacks[serial!].Invoke(json);
                SerialCallbacks.Remove(serial!);
                return;
            }
        }
        commandHandlerMethod.Invoke(json, encrypted);
        return;
    }
    
    /// <summary>
    /// We read the data from the stream, and then we check if we have enough data to read a packet. If we do, we read the
    /// packet, and then we remove it from the buffer. If we don't, we wait for more data
    /// </summary>
    /// <param name="IAsyncResult">The result of the asynchronous operation.</param>
    /// <returns>
    /// The number of bytes read from the stream.
    /// </returns>
    private void OnRead(IAsyncResult readResult)
    {
        try
        {
            var numberOfBytes = stream.EndRead(readResult);
            totalBuffer = Concat(totalBuffer, buffer, numberOfBytes);
        }
        catch(Exception e)
        {
            Logger.LogMessage(LogImportance.Error, "Error (Unknown Reason) ", e);
            OnDisconnect();
            return;
        }

        while (totalBuffer.Length >= 4)
        {
            var packetSize = BitConverter.ToInt32(totalBuffer, 0);

            if (totalBuffer.Length >= packetSize + 4) 
            {
                var json = Encoding.UTF8.GetString(totalBuffer, 4, packetSize);
                // Logger.LogMessage(LogImportance.DebugHighlight, "Received: '" + json.ToString() + "'");
                if (json.Length == 0)
                {
                    Logger.LogMessage(LogImportance.Error, "Incoming message length was 0");
                    return;
                }
                OnMessage?.Invoke(this, JObject.Parse(json));

                var newBuffer = new byte[totalBuffer.Length - packetSize - 4];
                Array.Copy(totalBuffer, packetSize + 4, newBuffer, 0, newBuffer.Length);
                totalBuffer = newBuffer;
            }

            else
                break;
        }

        stream.BeginRead(buffer, 0, 40960000, OnRead, null);
    }

    public virtual void OnDisconnect()
    {
        Logger.LogMessage(LogImportance.Fatal, "Disconnected with Server.");
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
    }
    /// <summary>
    /// It sends a message to the server
    /// </summary>
    /// <param name="message">The message to send to the server.</param>
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
        stream.Write(data, 0, data.Length);
        stream.Write(comman, 0, comman.Length);
    }
    
    /// <summary>
    /// It takes two byte arrays and a count, and returns a new byte array that is the concatenation of the first two
    /// arrays, with the second array truncated to the specified count
    /// </summary>
    /// <param name="b1">The first byte array to concatenate.</param>
    /// <param name="b2">The byte array to be appended to b1</param>
    /// <param name="count">The number of bytes to copy from the second array.</param>
    /// <returns>
    /// The concatenation of the two byte arrays.
    /// </returns>
    private static byte[] Concat(byte[] b1, byte[] b2, int count)
    {
        var r = new byte[b1.Length + count];
        Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
        Buffer.BlockCopy(b2, 0, r, b1.Length, count);
        return r;
    }
    
    /// <summary>
    /// It returns the public key of the RSA object
    /// </summary>
    /// <returns>
    /// The public key of the RSA object.
    /// </returns>
    public string GetRsaPublicKey()
    {
        return Rsa.ToXmlString(false);
    }
    
    /// <summary>
    /// > This function adds a callback to the serialCallbacks dictionary
    /// </summary>
    /// <param name="serial">The serial number of the device you want to listen for.</param>
    /// <param name="action">The function to be called when the serial is received.</param>
    public void AddSerialCallback(string serial, Action<JObject> action)
    {
        if (SerialCallbacks.ContainsKey(serial))
        {
            SerialCallbacks.Remove(serial);
        }
        
        SerialCallbacks.Add(serial, action);
    }

    public void RemoveSerialCallback(string serial)
    {
        SerialCallbacks.Remove(serial);
    }

    public async Task AddSerialCallbackTimeout(string serial, Action<JObject> action, Action timeoutAction, int timeout)
    {
        var received = false;
        JObject? json = null;
        AddSerialCallback(serial, ob =>
        {
            json = ob;
            received = true;
        });
        var task = Task.Delay(timeout);
        while (received == false && !task.IsCompleted)
        {
            await Task.Delay(1);
        }
        
        if (received)
        {
            action.Invoke(json!);
        }
        else
        {
            timeoutAction.Invoke();
        }
    }
    
    /// <summary>
    /// It encrypts the message with AES, encrypts the AES key and IV with RSA, and sends the encrypted message
    /// </summary>
    /// <param name="String">The message to send</param>
    public void SendEncryptedData(String message, bool hide = false)
    {
        
        try
        {
            if(!hide)
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
        newRsa.FromXmlString(publicKey);

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
            }, JsonFolderShared.Json.Path), hide);
        }
        else
        {
            Logger.LogMessage(LogImportance.Warn, $"Could not send EncryptedData, something went wrong with encrypting the aes-keys");
        }
    }
    #endregion

    /// <summary>
    /// It closes the stream and the client
    /// </summary>
    public void Disconnect()
    {
        stream.Close();
        client.Close();
    }
}