using Newtonsoft.Json;
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
    [OneTimeSetUp]
    public void Setup()
    {
        patientCommandHandler = new Dictionary<string, ICommandHandler>()
        {
            {"public-rsa-key", new RsaKey()},
            {"encryptedMessage", new EncryptedMessage()}
        };
        
        doctorCommandHandler = new Dictionary<string, ICommandHandler>()
        {
            {"public-rsa-key", new RsaKey()},
            {"encryptedMessage", new EncryptedMessage()}
        };

        patientUserName = "OnlyFolder";
        port = 2446;
        server = new Server(port);
        patient = new DefaultClientConnection("127.0.0.1", port, json =>
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

        doctor = new DefaultClientConnection("127.0.0.1", port, json =>
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

    [Test]
    public void Test1TwoUsersConnected()
    {
        Assert.That(server.users.Count, Is.EqualTo(2), "Could not establish a connection between the server and patient and between server and doctor.");
        Assert.Pass("Server was able to make a connection to doctor and patient");
    }

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

    [Test]
    public void Test4CheckActiveClientsCommand()
    {
        Thread.Sleep(1000);
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
        
        Thread.Sleep(500);
        if (passed)
        {
            Assert.Pass("Patient was in ActiveUser list");
        }
    }
}