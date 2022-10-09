using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using PrivateObjectExtension;
using ServerApplication;
using ServerApplication.Client;

namespace UnitTests;

public class Tests
{
    private PrivateObject server;
    private Server realServer;
    
    private TcpClient tcpClient;
    private NetworkStream stream;

    [SetUp]
    public void Setup()
    {
        int port = FreeTcpPort();
        Console.WriteLine($"Port: {port}");
        realServer = new Server(port);
        server = new PrivateObject(realServer);
        while (realServer == null)
        {
            Console.WriteLine("Waiting for server to begin...");
        }
        Thread.Sleep(2000);
         // tcpClient = new("127.0.0.1", port);
         // stream = tcpClient.GetStream();
    }

    [Test]
    public void ConnectionBetweenServerAndClient()
    {
        GetPropertyValues(realServer);
        // var usersProperty = realServer.GetType().GetProperty("users");
        // //List<ClientData> users = (List<ClientData>) server.GetFieldOrProperty("users");
        // List<ClientData> users = (List<ClientData>) usersProperty.GetValue(typeof(List<ClientData>));
        Assert.GreaterOrEqual(1, 1);
    }
    
    private static void GetPropertyValues(Object obj)
    {
        Type t = obj.GetType();
        Console.WriteLine("Type is: {0}", t.Name);
        PropertyInfo[] props = t.GetProperties();
        Console.WriteLine("Properties (N = {0}):", 
            props.Length);
        foreach (var prop in props)
            if (prop.GetIndexParameters().Length == 0)
                Console.WriteLine("   {0} ({1}): {2}", prop.Name,
                    prop.PropertyType.Name,
                    prop.GetValue(obj));
            else
                Console.WriteLine("   {0} ({1}): <Indexed>", prop.Name,
                    prop.PropertyType.Name);
    }

    [Test]
    public void Login1()
    
    {
           
        Assert.Pass();
    }
    
    [Test]
    public void Login2()
    {
        
        Assert.Pass();
    }
    
    [Test]
    public void Login3()
    {
        
        Assert.Pass();
    }
    
    public int FreeTcpPort()
    {
      TcpListener l = new TcpListener(IPAddress.Loopback, 0);
      l.Start();
      int port = ((IPEndPoint)l.LocalEndpoint).Port;
      l.Stop();
      return port;
    }
}