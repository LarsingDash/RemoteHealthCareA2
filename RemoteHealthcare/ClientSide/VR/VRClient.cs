using System;
using System.Linq;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClientSide.VR;

public class VRClient
{
    //Session List
    private JObject savedSession = null;
    private DateTime savedSessionDate;

    //Tunnel
    private TcpClient tcpClient = new();
    private NetworkStream stream;
    public string TunnelID { get; private set; }
    private Tunnel Tunnel { get; }

    //Data buffers for stream
    private byte[] totalBuffer = Array.Empty<byte>();
    private readonly byte[] buffer = new byte[1024];

    //Settings
    private World selectedWorld = World.forest;

    //Other
    List<string> removalTargets = new List<string>();

    public VRClient()
    {
        Tunnel = new Tunnel(this);
        //commands.Add("tunnel/create", new TunnelCreate());
        //commands.Add("tunnel/send", tunnel = new Tunnel(this));
    }

    /// <summary>
    /// It creates a tunnel with the given id
    /// </summary>
    /// <param name="id">The ID of the tunnel you want to create.</param>
    private void CreateTunnel(string id)
    {
        Console.WriteLine($"ID: {id}");
        SendData(JsonFileReader.GetObjectAsString("CreateTunnel", new Dictionary<string, string>()
        {
            {"_id_", id}
        }));
    }

    /// <summary>
    /// Its sets the tunnelID
    /// </summary>
    /// <param name="id">The id of the tunnel.</param>
    /// <returns>
    /// The tunnel id is being returned.
    /// </returns>
    public void TunnelStartup(string id)
    {
        TunnelID = id;

        //Remove groundPlane
        RemoveObjectRequest("GroundPlane");
    }

    //It connects to the server, gets the stream, and starts reading the stream
    //Then it asks for all sessions to find the correct one in the response
    public async Task StartConnectionAsync()
    {
        try
        {
            await tcpClient.ConnectAsync("145.48.6.10", 6666);
            stream = tcpClient.GetStream();
            stream.BeginRead(buffer, 0, 1024, OnRead, null);

            SendData(JsonFileReader.GetObjectAsString("SessionList", new Dictionary<string, string>()));
        }
        catch
        {
            Console.WriteLine("Could not connect with VRServer...");
        }
    }

    /// <summary>
    /// It takes a string, converts it to a byte array, and sends it to the server
    /// </summary>
    /// <param name="String">The string to send to the server</param>
    public void SendData(String s)
    {
        Console.WriteLine("-------------------------------------------Send Start");
        Console.WriteLine($"Sending data:\n{s}");

        byte[] data = BitConverter.GetBytes(s.Length);
        byte[] command = System.Text.Encoding.ASCII.GetBytes(s);

        stream.Write(data, 0, data.Length);
        stream.Write(command, 0, command.Length);

        Console.WriteLine("-------------------------------------------Send End");
    }

    /// <summary>
    /// It reads data from the stream, and if it has enough data to read a packet, it reads the packet and calls the
    /// appropriate command handler
    /// </summary>
    /// <param name="IAsyncResult">This is the result of the async operation.</param>
    /// <param name="ar"></param>
    /// <returns>
    /// The data that was sent from the server.
    /// </returns>
    private void OnRead(IAsyncResult ar)
    {
        try
        {
            int rc = stream.EndRead(ar);
            totalBuffer = Concat(totalBuffer, buffer, rc);
        }
        catch (System.IO.IOException)
        {
            Console.WriteLine("Error");
            return;
        }
        while (totalBuffer.Length >= 4)
        {
            int packetSize = BitConverter.ToInt32(totalBuffer, 0);
            if (totalBuffer.Length >= packetSize + 4)
            {
                string data = Encoding.UTF8.GetString(totalBuffer, 4, packetSize);
                JObject jData = JObject.Parse(data);
                Tunnel.HandleResponse(this, jData);
                var newBuffer = new byte[totalBuffer.Length - packetSize - 4];
                Array.Copy(totalBuffer, packetSize + 4, newBuffer, 0, newBuffer.Length);
                totalBuffer = newBuffer;
            }
            else
                break;
        }
        stream.BeginRead(buffer, 0, 1024, OnRead, null);
    }

    /// <summary>
    /// It takes two byte arrays and a count, and returns a new byte array that is the concatenation of the first two
    /// arrays, with the second array truncated to the specified count
    /// </summary>
    /// <param name="b1">The first byte array to concatenate.</param>
    /// <param name="b2">The byte array to be appended to b1</param>
    /// <param name="count">The number of bytes to copy from the second array.</param>
    /// <returns>
    /// The concatenated byte array.
    /// </returns>
    private static byte[] Concat(byte[] b1, byte[] b2, int count)
    {
        byte[] r = new byte[b1.Length + count];
        System.Buffer.BlockCopy(b1, 0, r, 0, b1.Length);
        System.Buffer.BlockCopy(b2, 0, r, b1.Length, count);
        return r;
    }

    public void ListSessions(JObject json)
    {
        //Go through all sessions in the json and find the one matching this systems host and user
        foreach (JObject currentObject in json["data"])
        {
            string? host = currentObject["clientinfo"]["host"].ToObject<string>();
            string? user = currentObject["clientinfo"]["user"].ToObject<string>();

            //Make sure neither are null
            if (host == null || user == null) continue;

            //Check if the host and user correspond to the systems host and user
            if (host.ToLower().Contains(Environment.MachineName.ToLower()) &&
                user.ToLower().Contains(Environment.UserName.ToLower()))
            {
                //Save the session object if there wasn't one saved already or if this one is newer
                if (savedSession == null)
                {
                    savedSession = currentObject;
                    savedSessionDate = CustomParseDate(currentObject);
                }
                else
                {
                    if (savedSessionDate < CustomParseDate(currentObject))
                    {
                        savedSession = currentObject;
                        savedSessionDate = CustomParseDate(currentObject);
                    }
                }
            }
        }

        //If a session with the correct host and user was found create a tunnel
        if (savedSession != null)
        {
            CreateTunnel(savedSession["id"].ToObject<string>());
        }
        else
        {
            Console.WriteLine("Could not find user...");
        }
    }

    //Helper method for ListSessions()
    private DateTime CustomParseDate(JObject o)
    {
        return DateTime.ParseExact(o["lastPing"].ToObject<string>(), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
    }

    //Add the targeted object to the list of objects-to-remove and send a request to find that object
    private void RemoveObjectRequest(params string[] targets)
    {
        foreach (var target in targets) removalTargets.Add(target);

        Tunnel.SendTunnelMessage(new Dictionary<string, string>()
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\GetScene",
            new Dictionary<string, string>())},
        });
    }

    //Loop through the json to find all objects that have a name that is in the removalTargets list, once a target has been found, request its removal
    public void RemoveObject(JObject json)
    {
        try
        {
            foreach (JObject currentObject in json["data"]["data"]["data"]["children"])
            {
                string? name = currentObject["name"].ToObject<string>();
                if (name == null) continue;

                if (!removalTargets.Contains(name)) continue;
                //Send a message to remove the node with the found uuid
                string? uuid = currentObject["uuid"].ToObject<string>();

                Tunnel.SendTunnelMessage(new Dictionary<string, string>()
                {
                    {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\DeleteNodeScene",
                        new Dictionary<string, string>())},
                    {"_id_", uuid}
                });
            }
        }
        catch
        {
            Console.WriteLine("No GroundPlane found, already removed?");
        }
    }
}