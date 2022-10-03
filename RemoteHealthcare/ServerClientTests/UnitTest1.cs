using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerApplication;
using ServerApplication.Client;
using ServerClientTests.UtilClasses.CommandHandlers;
using Shared;
using Shared.Encryption;
using Formatting = Newtonsoft.Json.Formatting;
using Shared.Log;

namespace ServerClientTests;

public class Tests
{
    private Server server;
    private FieldInfo usersFieldServer;
    private Type serverType;
    private ClientData data;
    private DefaultClientConnection client;
    private Dictionary<string, ICommandHandler> commandHandler;

    #region client
    //private TcpClient client;
    private NetworkStream stream;
    private Dictionary<string, Action<JObject>> serialCallbacks = new();
    private byte[] PublicKey;
    #endregion
    
    [SetUp]
    public void Setup()
    {
        commandHandler = new Dictionary<string, ICommandHandler>()
        {
            {"public-rsa-key", new RsaKey()},
            {"encryptedMessage", new EncryptedMessage()}
        };

        server = new Server(2450);
        Thread.Sleep(200);
        
        serverType = typeof(Server);
        usersFieldServer = serverType.GetField("users", BindingFlags.NonPublic | BindingFlags.Instance)!;

        client = new DefaultClientConnection("127.0.0.1", 2450, (json, encrypted) =>
        {
            if (commandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
            {
                if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
                {
                    Logger.LogMessage(LogImportance.Information, $"Got message from server: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
                }
                commandHandler[json["id"]!.ToObject<string>()!].HandleCommand(client, json);
            }
            else
            {
                Logger.LogMessage(LogImportance.Warn, $"Got message from server but no commandHandler found: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
            }
        });
        Logger.LogMessage(LogImportance.Fatal, "Test");
        
        Thread.Sleep(500);

    }
    [Test]
    public void TestConnectionAndCommands()
    {
        TestConnectionServerAndClient();
        RsaKey();
        TestLogin();
        TestLogin2();
        
        //var uuid = StartBikeRecordings();
        // AddValuesToBikeRecording(uuid);
        // StopBikeRecording(uuid);
        // CheckBikeRecordingFile(uuid);

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
        client.AddSerialCallback(serial, ob =>
        {
            PublicKey = ob["data"].Value<JArray>("key").Values<byte>().ToArray();
            passed = true;
           // Assert.Pass();
        });
        
        client.SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", new Dictionary<string, string>()
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
        client.AddSerialCallback(serial, ob =>
        {
            Assert.AreNotEqual(ob["data"]!["status"]!.ToObject<string>()!, "ok", "Status was ok when type was wrong");
            passed = true;
        });
        
        client.SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
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
        client.AddSerialCallback(serial, ob =>
        {
            Assert.AreEqual(ob["data"]!["status"]!.ToObject<string>()!, "ok", "Status was not ok when logging in");
            passed = true;
        });
        
        client.SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
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
        client.AddSerialCallback(serial, ob =>
        {
            if (ob["data"]["status"].ToObject<string>().Equals("ok"))
            {
                uuid = ob["data"]["uuid"].ToObject<string>();
            }
            Assert.AreEqual("ok", ob["data"]!["status"]!.ToObject<string>()!, "Could not start Bike recording. Error: " + ob["data"]?["error"]?.ToObject<string>());
            passed = true;
        });
        client.SendData(JsonFileReader.GetObjectAsString("StartBikeRecording", new Dictionary<string, string>()
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
        
        client.AddSerialCallback(serial, ob =>
        {
            Assert.AreEqual("ok", ob["data"]!["status"]!.ToObject<string>()!, "Could not add data: " + ob["data"]!["error"]!.ToObject<string>()!);
            passed = true;
        });
        client.SendData(JsonFileReader.GetObjectAsString("ChangeData", new Dictionary<string, string>()
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
        
        client.AddSerialCallback(serial, ob =>
        {
            Assert.AreEqual("ok", ob["data"]!["status"]!.ToObject<string>()!, "Could not stop bike recording: " + ob["data"]!["error"]!.ToObject<string>()!);
            passed = true;
        });
        
        client.SendData(JsonFileReader.GetObjectAsString("StopBikeRecording", new Dictionary<string, string>()
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
}