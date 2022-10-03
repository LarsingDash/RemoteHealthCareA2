using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using ServerApplication;
using ServerApplication.Client;
using Shared;
using Shared.Encryption;
using Formatting = Newtonsoft.Json.Formatting;

namespace ServerClientTests;

public class Tests
{
    private Server server;
    private FieldInfo usersFieldServer;
    private Type serverType;
    private ClientData data;

    #region client
    private TcpClient client;
    private NetworkStream stream;
    private Dictionary<string, Action<JObject>> serialCallbacks = new();
    private byte[] PublicKey;
    #endregion
    
    [SetUp]
    public void Setup()
    {
        OnMessage += async (_, json) => await ProcessMessageAsync(json);
        
        server = new Server(2450);
        Thread.Sleep(200);
        
        serverType = typeof(Server);
        usersFieldServer = serverType.GetField("users", BindingFlags.NonPublic | BindingFlags.Instance)!;

        client = new("127.0.0.1", 2450);
        stream = client.GetStream();
        stream.BeginRead(_buffer, 0, 1024, OnRead, null);
        
    }
    [Test]
    public void TestConnectionAndCommands()
    {
        TestConnectionServerAndClient();
        RsaKey();
        TestLogin();
        TestLogin2();
        
        var uuid = StartBikeRecordings();
        AddValuesToBikeRecording(uuid);
        StopBikeRecording(uuid);
        CheckBikeRecordingFile(uuid);

    }
    #region Connection with Server
    public void TestConnectionServerAndClient()
    {
        List<ClientData> dataL = server.users;
        if (dataL.Count > 0)
        {
            this.data = dataL[0];
        }
        Assert.GreaterOrEqual(1, dataL.Count, "No connection between server and client");
    }
    #endregion
    #region Getting Rsa Key
    public void RsaKey()
    {
        var serial = Util.RandomString();
        var passed = false;
        AddSerialCallback(serial, ob =>
        {
            PublicKey = ob["data"].Value<JArray>("key").Values<byte>().ToArray();
            passed = true;
           // Assert.Pass();
        });
        
        SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", new Dictionary<string, string>()
        {
            {"_serial_", serial}
        }));
        Thread.Sleep(500);
        Assert.IsTrue(passed, "No RSA Key received from server.");
    }
    #endregion
    #region Login Command
    public void TestLogin()
    {
        var serial = Util.RandomString();
        var passed = false;
        AddSerialCallback(serial, ob =>
        {
            Assert.AreNotEqual(ob["data"]!["status"]!.ToObject<string>()!, "ok", "Status was ok when type was wrong");
            passed = true;
        });
        
        SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
        {
            {"_type_", "noType"},
            {"_username_", "TestClient53242"},
            {"_serial_", serial}
        }, JsonFolder.Json.Path));
        
        Thread.Sleep(500);
        Assert.IsTrue(passed, "No Response from Command login received.");
    }

    public void TestLogin2()
    {
        var serial = Util.RandomString();
        var passed = false;
        AddSerialCallback(serial, ob =>
        {
            Assert.AreEqual(ob["data"]!["status"]!.ToObject<string>()!, "ok", "Status was not ok when logging in");
            passed = true;
        });
        
        SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
        {
            {"_type_", "Client"},
            {"_username_", "OnlyFolder"},
            {"_serial_", serial}
        }, JsonFolder.Json.Path));
        
        Thread.Sleep(500);
        Assert.IsTrue(passed, "No Response from Command login received.");
    }
    #endregion
    
    #region Bike Recordings
    public string StartBikeRecordings()
    {
        var serial = Util.RandomString();
        var passed = false;
        var uuid = "";
        AddSerialCallback(serial, ob =>
        {
            if (ob["data"]["status"].ToObject<string>().Equals("ok"))
            {
                uuid = ob["data"]["uuid"].ToObject<string>();
            }
            Assert.AreEqual("ok", ob["data"]!["status"]!.ToObject<string>()!, "Could not start Bike recording. Error: " + ob["data"]?["error"]?.ToObject<string>());
            passed = true;
        });
        SendData(JsonFileReader.GetObjectAsString("StartBikeRecording", new Dictionary<string, string>()
        {
            {"_session_", "TestSession"},
            {"_serial_", serial}
        }, JsonFolder.Json.Path));
        
        Thread.Sleep(500);
        Assert.IsTrue(passed, "No Response from start-bike-recording.");

        return uuid;

    }

    public void AddValuesToBikeRecording(string uuid)
    {
        var serial = Util.RandomString();
        var passed = false;
        
        AddSerialCallback(serial, ob =>
        {
            Assert.AreEqual("ok", ob["data"]!["status"]!.ToObject<string>()!, "Could not add data: " + ob["data"]!["error"]!.ToObject<string>()!);
            passed = true;
        });
        SendData(JsonFileReader.GetObjectAsString("ChangeData", new Dictionary<string, string>()
        {
            {"_serial_", serial},
            {"_uuid_", uuid}
        }, JsonFolder.Json.Path));
        Thread.Sleep(500);
        Assert.IsTrue(passed, "No Response change-data.");
    }

    public void StopBikeRecording(string uuid)
    {
        var serial = Util.RandomString();
        var passed = false;
        
        AddSerialCallback(serial, ob =>
        {
            Assert.AreEqual("ok", ob["data"]!["status"]!.ToObject<string>()!, "Could not stop bike recording: " + ob["data"]!["error"]!.ToObject<string>()!);
            passed = true;
        });
        
        SendData(JsonFileReader.GetObjectAsString("StopBikeRecording", new Dictionary<string, string>()
        {
            {"_serial_", serial},
            {"_uuid_", uuid}
        }, JsonFolder.Json.Path));
        
        Thread.Sleep(500);
        Assert.IsTrue(passed, "No Response stop-bike-recording.");
    }

    public void CheckBikeRecordingFile(string uuid)
    {
        string file = JsonFileReader.GetEncryptedText(uuid + ".txt", new Dictionary<string, string>(),
            JsonFolder.Data + this.data.UserName + "\\");
        Console.WriteLine(JObject.Parse(file).ToString());
        Assert.IsTrue(file.Contains("time2"), "Change data has not been written to file.");
        Assert.IsFalse(file.Contains("_starttime_"), "Starttime has not changed in file. Check start-bike-recording");
        Assert.IsFalse(file.Contains("_endtime_"), "Endtime has not changed in file. Check end-bike-recording");
    }
    
    public long CountLinesLinq(FileInfo file)  
        => File.ReadLines(file.FullName).Count();
    
    #endregion
    #region Client Connection stuff
    private RSA rsa = new RSACryptoServiceProvider();
    private byte[] _totalBuffer = Array.Empty<byte>();
    private readonly byte[] _buffer = new byte[1024];
    public event EventHandler<JObject> OnMessage;
    
    private async Task ProcessMessageAsync(JObject json)
    {
        Console.WriteLine($"Received: {json.ToString(Formatting.None)}");
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
                Console.WriteLine("Encrypted Message");
                var keyCrypted = json["aes-keys"].Value<JArray>("Key").Values<byte>().ToArray();
                var iVCrypted = json["aes-keys"].Value<JArray>("IV").Values<byte>().ToArray();
                    
                var messageCrypted = json.Value<JArray>("aes-data").Values<byte>().ToArray();
                    
                var key = RsaHelper.DecryptMessage(keyCrypted, rsa.ExportParameters(true), false);
                var iV = RsaHelper.DecryptMessage(iVCrypted, rsa.ExportParameters(true), false);
                
                var message = AesHelper.DecryptMessage(messageCrypted, key, iV);
                ProcessMessageAsync(JObject.Parse(message));
                break;
            }
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
    
    private void SendData(String s)
    {
        Console.WriteLine($"Sending data to server: {JObject.Parse(s).ToString(Formatting.None)}");
        Byte[] data = BitConverter.GetBytes(s.Length);
        Byte[] comman = System.Text.Encoding.ASCII.GetBytes(s);
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
        Console.WriteLine("Send Encrypted Message...");
        Aes aes = Aes.Create("AesManaged") ?? new AesManaged();
        RSA rsa = new RSACryptoServiceProvider();
        rsa.ImportRSAPublicKey(PublicKey, out int a);

        var keyCrypt = RsaHelper.EncryptMessage(aes.Key, rsa.ExportParameters(false), false);
        var IVCrypt = RsaHelper.EncryptMessage(aes.IV, rsa.ExportParameters(false), false);
        var aesCrypt = AesHelper.EncryptMessage(message, aes.Key, aes.IV);

        var serial = Util.RandomString();

        SendData(JsonFileReader.GetObjectAsString("EncryptedMessage", new Dictionary<string, string>()
        {
            {"\"_IV_\"", Util.ByteArrayToString(IVCrypt)},
            {"\"_key_\"", Util.ByteArrayToString(keyCrypt)},
            {"\"_data_\"", Util.ByteArrayToString(aesCrypt)},
            {"_serial_", serial}
        }, JsonFolder.Json.Path));
    }
    #endregion
    
    
}