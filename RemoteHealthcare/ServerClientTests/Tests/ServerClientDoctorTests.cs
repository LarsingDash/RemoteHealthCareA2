using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerApplication;
using ServerClientTests.UtilClasses.CommandHandlers;
using Shared;
using Shared.Log;

namespace ServerClientTests;

public class ServerClientDoctorTests
{
    private DefaultClientConnection patient;
    private Dictionary<string, ICommandHandler> patientCommandHandler;
    
    private DefaultClientConnection doctor;
    private Dictionary<string, ICommandHandler> doctorCommandHandler;
    private Server server;

    private int port;
    private string patientUserName;
    
    /// <summary>
    /// It creates a server, two clients, and a dictionary of command handlers for each client
    /// </summary>
    [OneTimeSetUp]
    public void Setup()
    {
        patientCommandHandler = new Dictionary<string, ICommandHandler>()
        {
            {"public-rsa-key", new RsaKey()},
            {"encryptedMessage", new EncryptedMessage()},
            {"forward-chat-message", new ChatMessage()}
        };
        
        doctorCommandHandler = new Dictionary<string, ICommandHandler>()
        {
            {"public-rsa-key", new RsaKey()},
            {"encryptedMessage", new EncryptedMessage()}
        };

        patientUserName = "OnlyFolder";
        port = 2446;
        server = new Server(port);
        patient = new DefaultClientConnection("127.0.0.1", port, (json, encrypted) =>
        {
            if (patientCommandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
            {
                if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
                {
                    Logger.LogMessage(LogImportance.Information, $"Got message from server: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
                }
                patientCommandHandler[json["id"]!.ToObject<string>()!].HandleCommand(patient, json);
            }
            else
            {
                Logger.LogMessage(LogImportance.Warn, $"Got message from server but no commandHandler found: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
            }
        });

        doctor = new DefaultClientConnection("127.0.0.1", port, (json, encrypted) =>
        {
            if (doctorCommandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
            {
                if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
                {
                    Logger.LogMessage(LogImportance.Information, $"Got message from server: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
                }
                doctorCommandHandler[json["id"]!.ToObject<string>()!].HandleCommand(doctor, json);
            }
            else
            {
                Logger.LogMessage(LogImportance.Warn, $"Got message from server but no commandHandler found: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
            }
        });
        
        Thread.Sleep(500);
    }

    /// <summary>
    /// This function tests to see if the server can make a connection to the doctor and patient
    /// </summary>
    [Test]
    public void Test1TwoUsersConnected()
    {
        Assert.That(server.users.Count, Is.EqualTo(2), "Could not establish a connection between the server and patient and between server and doctor.");
        Assert.Pass("Server was able to make a connection to doctor and patient");
    }

    /// <summary>
    /// We send a login request to the server, and wait for a response
    /// </summary>
    [Test]
    public void Test2LoginClient()
    {
        var serial = Util.RandomString();
        var passed = false;
        patient.AddSerialCallback(serial, ob =>
        {
            Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"), "Status was not ok when logging in");
            passed = true;
        });
        
        patient.SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
        {
            {"_type_", "Client"},
            {"_username_", patientUserName},
            {"_serial_", serial}
        }, JsonFolder.Json.Path));
        
        Thread.Sleep(500);
        Assert.That(passed, Is.EqualTo(true), "No Response from Command login received.");
        Assert.Pass("Patient logged in.");
    }
    
    /// <summary>
    /// We create a random string, which we use as a serial. We then add a callback to the doctor object, which will be
    /// called when the server responds with the same serial. We then send the login command to the server, and wait for the
    /// callback to be called
    /// </summary>
    [Test]
    public void Test3LoginDoctor()
    {
        var serial = Util.RandomString();
        var passed = false;
        doctor.AddSerialCallback(serial, ob =>
        {
            Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"), "Status was not ok when logging in");
            passed = true;
        });
        
        doctor.SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
        {
            {"_type_", "Doctor"},
            {"_username_", "Jasper"},
            {"_password_", "Merijn"},
            {"_serial_", serial}
        }, JsonFolder.Json.Path));
        
        Thread.Sleep(500);
        Assert.That(passed, Is.EqualTo(true), "No Response from Command login received.");
        Assert.Pass("Doctor logged in.");
    }

    /// <summary>
    /// The doctor sends a request to the server to get a list of all active users. The server responds with a list of all
    /// active users. The doctor checks if the patient is in the list
    /// </summary>
    [Test]
    public void Test4CheckActiveClientsCommand()
    {
        Thread.Sleep(100);
        var serial = Util.RandomString();
        doctor.SendEncryptedData(JsonFileReader.GetObjectAsString("ActiveClients", new Dictionary<string, string>()
        {
            {"_serial_", serial}
        }, JsonFolder.Json.Path));

        var passed = false;
        doctor.AddSerialCallback(serial, json =>
        {
            Assert.That(json.ToString(Formatting.None).Contains(patientUserName), Is.True, "Patient was not found in ActiveUser list");
            passed = true;
        });
        
        Thread.Sleep(250);
        Assert.That(passed, Is.EqualTo(true), "No Response from Command activeUsers received.");
        if (passed)
        {
            Assert.Pass("Patient was in ActiveUser list");
        }
    }

    [Test]
    public void Test5CheckChatMessage()
    {
        Thread.Sleep(100);
        var serial = Util.RandomString();
        doctor.SendEncryptedData(JsonFileReader.GetObjectAsString("ChatMessage", new Dictionary<string, string>()
        {
            {"_serial_", serial},
            {"_message_", "Testbericht"},
            {"_receiver_", patientUserName}
        }, JsonFolder.Json.Path));
        var passed = false;
        doctor.AddSerialCallback(serial, json =>
        {
            if (json["data"]!["status"]!.ToObject<string>()!.Equals("ok"))
            {
                passed = true;
            }
            Assert.That(json["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"), "Did not receive a responds from server");
        });
        
        Thread.Sleep(250);
        Assert.That(passed, Is.EqualTo(true), "No Response from Command chat-message received.");
        if (passed)
        {
            Assert.Pass("ChatMessage send to server, got status ok response");
        }
    }

    [Test]
    public void Test6CheckChatMessageForward()
    {   
        Thread.Sleep(100);
        Assert.That(ChatMessage.received, Is.GreaterThan(0), "Patient did not get the chat message from doctor");
        Assert.Pass("Patient received chat message from doctor");
    }

    private string uuid;
    [Test]
    public void Test7StartBikeRecording()
    {
        Thread.Sleep(100);
        var serial = Util.RandomString();
        var passed = false;
        uuid = "";
        doctor.AddSerialCallback(serial, ob =>
        {
            if (ob["data"]!["status"]!.ToObject<string>()!.Equals("ok"))
            {
                uuid = ob["data"]!["uuid"]!.ToObject<string>()!;
                passed = true;
            }
            Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"), "Could not start Bike recording. Error: " + ob["data"]?["error"]?.ToObject<string>());
        });
        doctor.SendData(JsonFileReader.GetObjectAsString("StartBikeRecording", new Dictionary<string, string>()
        {
            {"_session_", "TestSession"},
            {"_serial_", serial},
            {"_name_", patientUserName}
        }, JsonFolder.Json.Path));

        Thread.Sleep(250);
        Assert.That(passed, Is.True, "No Response from Command start-bike-recording received.");
        Assert.Pass("Received uuid from start-bike-recording");
    }

    [Test]
    public void Test8ChangeBikeValues()
    {
        Thread.Sleep(100);
        var serial = Util.RandomString();
        var passed = false;
        
        patient.AddSerialCallback(serial, ob =>
        {
            Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"), "Could not add data: " + ob["data"]!["error"]!.ToObject<string>()!);
            passed = true;
        });
        patient.SendData(JsonFileReader.GetObjectAsString("ChangeData", new Dictionary<string, string>()
        {
            {"_serial_", serial},
            {"_uuid_", uuid}
        }, JsonFolder.Json.Path));
        Thread.Sleep(500);
        Assert.That(passed, Is.True, "No Response from Command change-bike-recording received.");
        Assert.Pass("Values for the bike recording have been changed.");
    }
    
    [Test]
    public void Test90StopBikeRecording()
    {
        Thread.Sleep(100);
        var serial = Util.RandomString();
        var passed = false;
        
        patient.AddSerialCallback(serial, ob =>
        {
            Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"), "Could not stop bike recording: " + ob["data"]!["error"]!.ToObject<string>()!);
            passed = true;
        });
        patient.SendData(JsonFileReader.GetObjectAsString("StopBikeRecording", new Dictionary<string, string>()
        {
            {"_serial_", serial},
            {"_uuid_", uuid}
        }, JsonFolder.Json.Path));
        Thread.Sleep(500);
        Assert.That(passed, Is.True, "No Response from Command stop-bike-recording received.");
        Assert.Pass("Bike Recording has been stopped.");
    }
    
    [Test]
    public void Test91CheckBikeRecordingFile()
    {
        Thread.Sleep(200);
        string file = JsonFileReader.GetEncryptedText(uuid + ".txt", new Dictionary<string, string>(),
            ServerApplication.UtilData.JsonFolder.Data + this.patientUserName + "\\");
        Assert.IsTrue(file.Contains("time2"), "Change data has not been written to file.");
        Assert.IsFalse(file.Contains("_starttime_"), "Starttime has not changed in file. Check start-bike-recording");
        Assert.IsFalse(file.Contains("_endtime_"), "Endtime has not changed in file. Check end-bike-recording");
        
        Assert.Pass("Bike Recording File is correct");
    }
}