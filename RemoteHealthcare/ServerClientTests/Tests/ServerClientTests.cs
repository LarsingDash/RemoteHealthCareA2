using Newtonsoft.Json;
using ServerApplication;
using ServerClientTests.UtilClasses.CommandHandlers;
using Shared;
using Shared.Log;

namespace ServerClientTests;

public class ServerClientTests
{
    private Server server;
    private int port;

    private Dictionary<string, ICommandHandler> clientCommandHandler;

    private DefaultClientConnection client;

    /// <summary>
    /// It creates a server and a client, and then waits for 500ms
    /// </summary>
    [OneTimeSetUp]
    public void Setup()
    {
        port = 2447;
        server = new Server(port);

        clientCommandHandler = new Dictionary<string, ICommandHandler>()
        {
            {"public-rsa-key", new RsaKey()},
            {"encryptedMessage", new EncryptedMessage()}
        };
        client = new DefaultClientConnection("127.0.0.1", port, (json, encrypted) => {
            if (clientCommandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
            {
                if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
                {
                    Logger.LogMessage(LogImportance.Information, $"Got message from server: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
                }
                clientCommandHandler[json["id"]!.ToObject<string>()!].HandleCommand(client, json);
            }
            else
            {
                Logger.LogMessage(LogImportance.Warn, $"Got message from server but no commandHandler found: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
            }
        });
        
        Thread.Sleep(500);
    }

    /// <summary>
    /// "TestConnection() tests the connection between the server and client."
    /// 
    /// The first line of the function is a comment. Comments are ignored by the compiler, but they are useful for
    /// explaining what the code does
    /// </summary>
    [Test]
    public void TestConnection()
    {
        Assert.That(server.users.Count, Is.GreaterThan(0), "Could not establish a connection between the server and client.");
        Assert.Pass("Server was able to make a connection");
    }

    /// <summary>
    /// The client waits for half a second, then requests the server's public RSA key. If the server's public RSA key is not
    /// empty, the test passes
    /// </summary>
    [Test]
    public void TestServerSendingRsaKey()
    {
        Thread.Sleep(500);
        string pKey = client.GetFieldValue<string>("PublicKey");
        Assert.That(pKey.Length, Is.GreaterThan(0), "Client got no response when requesting public RSA key of server.");
        Assert.Pass("Client received public RSA key of server.");
    }
    
    /// <summary>
    /// The server waits for half a second, then checks if the client has sent its public RSA key
    /// </summary>
    [Test]
    public void TestClientSendingRsaKey()
    {
        Thread.Sleep(500);
        string? pKey = server.users[0].GetPropertyValue<string?>("PublicKey");
        Assert.That(pKey?.Length ?? 0, Is.GreaterThan(0), "Server got no response when requesting public RSA key of client.");
        Assert.Pass("Server received public RSA key of client.");
    }
}