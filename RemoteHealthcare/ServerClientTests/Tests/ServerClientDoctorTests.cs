using System.Data;
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
            {"forward-chat-message", new ChatMessage()},
            {"forward-set-resistance", new SetResistance()}
        };
        
        doctorCommandHandler = new Dictionary<string, ICommandHandler>()
        {
            {"public-rsa-key", new RsaKey()},
            {"encryptedMessage", new EncryptedMessage()},
            {"update-values", new UpdatesValues()}
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
    public async Task Test2LoginClient()
    {
        var serial = Util.RandomString();
        patient.SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
        {
            {"_type_", "Client"},
            {"_username_", patientUserName},
            {"_password_", "OnlyFolder"},
            {"_serial_", serial}
        }, JsonFolder.Json.Path));
        await patient.AddSerialCallbackTimeout(serial, ob =>
            {
                Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"),
                    "Status was not ok when logging in");
            },
            () =>
            {
                Assert.Fail("No Response from Command login received.");
            }, 1000);
        Assert.Pass("Patient logged in.");
    }
    
    /// <summary>
    /// We create a random string, which we use as a serial. We then add a callback to the doctor object, which will be
    /// called when the server responds with the same serial. We then send the login command to the server, and wait for the
    /// callback to be called
    /// </summary>
    [Test]
    public async Task Test3LoginDoctor()
    {
        var serial = Util.RandomString();
        doctor.SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
        {
            {"_type_", "Doctor"},
            {"_username_", "Jasper"},
            {"_password_", "Merijn"},
            {"_serial_", serial}
        }, JsonFolder.Json.Path));
        
        await doctor.AddSerialCallbackTimeout(serial, ob =>
        {
            Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"),
                "Status was not ok when logging in");
        }, () =>
        {
            Assert.Fail("No Response from Command login received.");
        }, 1000);
        Assert.Pass("Doctor logged in.");
    }

    /// <summary>
    /// The doctor sends a request to the server to get a list of all active users. The server responds with a list of all
    /// active users. The doctor checks if the patient is in the list
    /// </summary>
    [Test]
    public async Task Test4CheckActiveClientsCommand()
    {
        Thread.Sleep(10);
        var serial = Util.RandomString();
        doctor.SendEncryptedData(JsonFileReader.GetObjectAsString("ActiveClients", new Dictionary<string, string>()
        {
            {"_serial_", serial}
        }, JsonFolder.Json.Path));
        await doctor.AddSerialCallbackTimeout(serial, ob =>
        {
            Assert.That(ob.ToString(Formatting.None).Contains(patientUserName), Is.True, "Patient was not found in ActiveUser list");
        }, () =>
        {
            Assert.Fail("No Response from Command activeUsers received.");
        }, 1000);
        Assert.Pass("Patient was in ActiveUser list");
    }

    [Test]
    public async Task Test5CheckChatMessage()
    {
        Thread.Sleep(10);
        var serial = Util.RandomString();
        doctor.SendEncryptedData(JsonFileReader.GetObjectAsString("ChatMessage", new Dictionary<string, string>()
        {
            {"_serial_", serial},
            {"_message_", "Testbericht"},
            {"_receiver_", patientUserName},
            {"_type_", "personal"}
        }, JsonFolder.Json.Path));
        await doctor.AddSerialCallbackTimeout(serial, ob =>
        {
            Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"), "Did not receive a responds from server");
            Assert.That(ChatMessage.received, Is.EqualTo(1), "Did not receive chat message");
        }, () =>
        {
            Assert.Fail("No Response from Command chat-message received.");
        }, 1000);
        
        doctor.SendEncryptedData(JsonFileReader.GetObjectAsString("ChatMessage", new Dictionary<string, string>()
        {
            {"_serial_", serial},
            {"_message_", "Testbericht1"},
            {"_type_", "broadcast"}
        }, JsonFolder.Json.Path));
        await doctor.AddSerialCallbackTimeout(serial, ob =>
        {
            Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"), "Did not receive a responds from server");
            Assert.That(ChatMessage.received, Is.EqualTo(2), "Did not receive chat message");
        }, () =>
        {
            Assert.Fail("No Response from Command chat-message received.");
        }, 1000);
        
        Assert.Pass("ChatMessage send to server, got status ok response");
        
        }

    [Test]
    public void Test6CheckChatMessageForward()
    {   
        Thread.Sleep(10);
        Assert.That(ChatMessage.received, Is.GreaterThan(0), "Patient did not get the chat message from doctor");
        Assert.Pass("Patient received chat message from doctor");
    }

    private string uuid = "";
    [Test]
    public async Task Test7StartBikeRecording()
    {
        var serial = Util.RandomString();
        doctor.SendData(JsonFileReader.GetObjectAsString("StartBikeRecording", new Dictionary<string, string>()
        {
            {"_session_", "TestSession"},
            {"_serial_", serial},
            {"_name_", patientUserName}
        }, JsonFolder.Json.Path));
        await doctor.AddSerialCallbackTimeout(serial, ob =>
        {
            uuid = ob["data"]!["uuid"]!.ToObject<string>()!;
            Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"), "Could not start Bike recording. Error: " + ob["data"]?["error"]?.ToObject<string>());
        }, () =>
        {
            Assert.Fail("No Response from Command start-bike-recording received.");
        }, 1000);
        Assert.Pass("Received uuid from start-bike-recording");
    }

    // [Test]
    // public async Task Test8ChangeBikeValues()
    // {
    //     if (uuid.Length == 0)
    //     {
    //         Assert.Fail("No uuid");
    //         return;
    //     }
    //     var serial = Util.RandomString();
    //
    //     patient.SendData(JsonFileReader.GetObjectAsString("ChangeData", new Dictionary<string, string>()
    //     {
    //         {"_serial_", serial},
    //         {"_uuid_", uuid}
    //     }, JsonFolder.Json.Path));
    //     await patient.AddSerialCallbackTimeout(serial, ob =>
    //     {
    //         Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"), "Could not add data: " + ob["data"]!["error"]!.ToObject<string>()!);
    //     }, () =>
    //     {
    //         Assert.Fail("No Response from Command change-bike-recording received.");
    //     }, 1000);
    //     Assert.Pass("Values for the bike recording have been changed.");
    // }
    //
    // [Test]
    // public async Task Test90StopBikeRecording()
    // {
    //     if (uuid.Length == 0)
    //     {
    //         Assert.Fail("No uuid");
    //         return;
    //     }
    //     var serial = Util.RandomString();
    //
    //     doctor.SendData(JsonFileReader.GetObjectAsString("StopBikeRecording", new Dictionary<string, string>()
    //     {
    //         {"_serial_", serial},
    //         {"_uuid_", uuid},
    //         {"_name_", patientUserName},
    //         {"_stopType_", "normal"}
    //     }, JsonFolder.Json.Path));
    //     await doctor.AddSerialCallbackTimeout(serial, ob =>
    //     {
    //         Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"), "Could not stop bike recording: " + ob["data"]!["error"]!.ToObject<string>()!);
    //     }, () =>
    //     {
    //         Assert.Fail("No Response from Command stop-bike-recording received.");
    //     }, 1000);
    //     Assert.Pass("Bike Recording has been stopped.");
    // }
    //
    // [Test]
    // public void Test91CheckBikeRecordingFile()
    // {
    //     Thread.Sleep(200);
    //     string file = JsonFileReader.GetEncryptedText(uuid + ".txt", new Dictionary<string, string>(),
    //         ServerApplication.UtilData.JsonFolder.Data + this.patientUserName + "\\");
    //     Assert.IsTrue(file.Contains("time2"), "Change data has not been written to file.");
    //     Assert.IsFalse(file.Contains("_starttime_"), "Starttime has not changed in file. Check start-bike-recording");
    //     Assert.IsFalse(file.Contains("_endtime_"), "Endtime has not changed in file. Check end-bike-recording");
    //     
    //     Assert.Pass("Bike Recording File is correct");
    // }
    
    // [Test]
    // public async Task Test92SubscribeToSession()
    // {
    //     Thread.Sleep(200);
    //     
    //     //Starting bike recording
    //     var serial = Util.RandomString();
    //     var uuid = "";
    //     doctor.SendData(JsonFileReader.GetObjectAsString("StartBikeRecording", new Dictionary<string, string>()
    //     {
    //         {"_session_", "TestSession1"},
    //         {"_serial_", serial},
    //         {"_name_", patientUserName}
    //     }, JsonFolder.Json.Path));
    //     await doctor.AddSerialCallbackTimeout(serial, ob =>
    //     {
    //         if (ob["data"]!["status"]!.ToObject<string>()!.Equals("ok"))
    //         {
    //             uuid = ob["data"]!["uuid"]!.ToObject<string>()!;
    //         }
    //         else
    //         {
    //             Assert.Fail("Could not start Bike recording. Error: " + ob["data"]?["error"]?.ToObject<string>());
    //         }
    //     }, () =>
    //     {
    //         Assert.Fail("Did not get a response from start-bike-recording");
    //     }, 1000);
    //
    //     //Subscribing to session
    //     serial = Util.RandomString();
    //     doctor.SendEncryptedData(JsonFileReader.GetObjectAsString("SubscribeToSession", new Dictionary<string, string>()
    //     {
    //         {"_uuid_", uuid},
    //         {"_serial_", serial}
    //     }));
    //     await doctor.AddSerialCallbackTimeout(serial, ob =>
    //     {
    //
    //     }, () =>
    //     {
    //         Assert.Fail("No response from subscribe-to-session");
    //     }, 1000);
    //     serial = Util.RandomString();
    //     patient.SendEncryptedData(JsonFileReader.GetObjectAsString("ChangeData", new Dictionary<string, string>()
    //     {
    //         {"_serial_", serial},
    //         {"_uuid_", uuid}
    //     }, JsonFolder.Json.Path));
    //     
    //     await patient.AddSerialCallbackTimeout(serial, ob =>
    //     {
    //         if (!ob["data"]!["status"]!.ToObject<string>()!.Equals("ok"))
    //         {
    //             Assert.Fail("Status was not true"  + ob!["data"]!["error"]!.ToObject<string>()!);
    //         }
    //         Assert.That(UpdatesValues.received, Is.EqualTo(1), "Did not receive update-values message from server");
    //     }, () =>
    //     {
    //         Assert.Fail("Could not subscribe to session: No response from server (ok / error)");
    //     }, 1000);
    //     Assert.Pass("Receiving update-values");
    // }
    //
    // [Test]
    // public async Task Test93UnsubscribeToSession()
    // {
    //     Thread.Sleep(200);
    //     
    //     //Starting bike recording
    //     var serial = Util.RandomString();
    //     var uuid = "";
    //     doctor.SendData(JsonFileReader.GetObjectAsString("StartBikeRecording", new Dictionary<string, string>()
    //     {
    //         {"_session_", "TestSession1"},
    //         {"_serial_", serial},
    //         {"_name_", patientUserName}
    //     }, JsonFolder.Json.Path));
    //     await doctor.AddSerialCallbackTimeout(serial, ob =>
    //     {
    //         if (ob["data"]!["status"]!.ToObject<string>()!.Equals("ok"))
    //         {
    //             uuid = ob["data"]!["uuid"]!.ToObject<string>()!;
    //         }
    //         else
    //         {
    //             Assert.Fail("Could not start Bike recording. Error: " + ob["data"]?["error"]?.ToObject<string>());
    //         }
    //     }, () =>
    //     {
    //         Assert.Fail("Did not get a response from start-bike-recording");
    //     }, 1000);
    //
    //     //Subscribing to session
    //     serial = Util.RandomString();
    //     doctor.SendEncryptedData(JsonFileReader.GetObjectAsString("SubscribeToSession", new Dictionary<string, string>()
    //     {
    //         {"_uuid_", uuid},
    //         {"_serial_", serial}
    //     }));
    //     await doctor.AddSerialCallbackTimeout(serial, ob =>
    //     {
    //
    //     }, () =>
    //     {
    //         Assert.Fail("No response from subscribe-to-session");
    //     }, 1000);
    //     serial = Util.RandomString();
    //     UpdatesValues.received = 0;
    //     serial = Util.RandomString();
    //     patient.SendEncryptedData(JsonFileReader.GetObjectAsString("ChangeData", new Dictionary<string, string>()
    //     {
    //         {"_serial_", serial},
    //         {"_uuid_", uuid}
    //     }, JsonFolder.Json.Path));
    //     doctor.SendEncryptedData(JsonFileReader.GetObjectAsString("UnSubscribeToSession", new Dictionary<string, string>()
    //     {
    //         {"_uuid_", uuid},
    //         {"_serial_", serial}
    //     }));
    //     await doctor.AddSerialCallbackTimeout(serial, ob =>
    //     {
    //
    //     }, () =>
    //     {
    //         Assert.Fail("No response from unsubscribe-to-session");
    //     }, 1000);
    //     serial = Util.RandomString();
    //     patient.SendEncryptedData(JsonFileReader.GetObjectAsString("ChangeData", new Dictionary<string, string>()
    //     {
    //         {"_serial_", serial},
    //         {"_uuid_", uuid}
    //     }, JsonFolder.Json.Path));
    //     
    //     await patient.AddSerialCallbackTimeout(serial, ob =>
    //     {
    //         if (!ob["data"]!["status"]!.ToObject<string>()!.Equals("ok"))
    //         {
    //             Assert.Fail("Status was not true"  + ob!["data"]!["error"]!.ToObject<string>()!);
    //         }
    //         Assert.That(UpdatesValues.received, Is.EqualTo(1), "Did not receive update-values message from server");
    //     }, () =>
    //     {
    //         Assert.Fail("Could not subscribe to session: No response from server (ok / error)");
    //     }, 1000);
    //     Assert.Pass("Receiving update-values");
    // }

    [Test]
    public async Task Test94SerialTimeout()
    {
        var serial = Util.RandomString();
        var done = false;
        patient.SendEncryptedData(JsonFileReader.GetObjectAsString("WrongJson", new Dictionary<string, string>()
        {
            {"_serial_", serial},
        }, JsonFolder.Json.Path));
        await patient.AddSerialCallbackTimeout(
            serial, 
            ob =>
        {
            
        },
            () =>
            {
                done = true;
            }, 
            1000);
        if (!done)
        {
            Assert.That(done, Is.True, "AddSerialCallbackTimeout is not working.");
            return;
        }
        
        Assert.Pass( "AddSerialCallbackTimeout is working");
    }
    
    [Test]
    public async Task Test95SerialTimeout()
    {
        var serial = Util.RandomString();
        var done = false;
        var millis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        patient.SendData(JsonFileReader.GetObjectAsString("PublicRSAKey", new Dictionary<string, string>()
        {
            {"_serial_", serial},
        }, JsonFolder.Json.Path));
        await patient.AddSerialCallbackTimeout(
            serial, 
            ob =>
            {
                done = true;
            },
            () =>
            {
            }, 
            500);
        
        if (!done && millis - DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond < 4500)
        {
            Assert.That(done, Is.True, "AddSerialCallbackTimeout is not working.");
            return;
        }
        Assert.Pass( "AddSerialCallbackTimeout is working");
    }
    
    [Test]
    public async Task Test96SetBikeResistanceDoctor()
    {
        var serial = Util.RandomString();

        doctor.SendData(JsonFileReader.GetObjectAsString("SetResistance", new Dictionary<string, string>()
        {
            {"_serial_", serial},
            {"_resistance_", "3"},
            {"_user_", patientUserName}
        }, JsonFolder.Json.Path));
        await doctor.AddSerialCallbackTimeout(serial, ob =>
        {
            Assert.That(ob["data"]!["status"]!.ToObject<string>()!, Is.EqualTo("ok"), "Could not set resistance: " + ob["data"]!["error"]!.ToObject<string>()!);
        }, () =>
        {
            Assert.Fail("No Response from Command set-resistance received.");
        }, 1000);
        Assert.That(SetResistance.received, Is.EqualTo(1), "Resistance change has not been received by client.");
        Assert.Pass("SetResistance has been received by server.");
    }
}