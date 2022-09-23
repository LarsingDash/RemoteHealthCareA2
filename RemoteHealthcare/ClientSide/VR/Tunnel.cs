using Newtonsoft.Json.Linq;

namespace ClientSide.VR;

public class Tunnel
{
    private VRClient vrClient;
    

    public Tunnel(VRClient vrClient)
    {
        this.vrClient = vrClient;
    }

    
    /// <summary>
    /// SendTunnelMessage sends a message to the VR client
    /// </summary>
    /// <param name="values"><para>A dictionary of key-value pairs that will be sent to the server. Needs to have the following values:</para>
    /// <para>_tunnelID_ = destination of the tunnel</para>
    /// <para>"_data_" = data of what to do. Example {"id": "scene/terrain/add", "value1":2}</para>
    /// </param>
    public void SendTunnelMessage(Dictionary<string, string> values)
    {
        values.Add("_tunnelID_", vrClient.tunnelID);
        vrClient.SendData(JsonFileReader.GetObjectAsString("SendTunnel", values));
    }
    

    public void HandleResponse(VRClient client, JObject json)
    {
        Console.WriteLine("------------------------------------------------------------Response Start");
        string? messageID;
        
        //Attempt to find the messageID and handle any exceptions
        try
        {
            messageID = json["id"].ToObject<string>();
            Console.WriteLine($"Message ID: {messageID}");
        }
        catch
        {
            Console.WriteLine("Something went wrong in finding the message ID");
            return;
        }

        if (messageID == null)
        {
            Console.WriteLine("Message ID was not found");
            return;
        }

        //Handle a response with status = error
        try
        {
            if (json.ContainsKey("status")) {
                string? status = json["status"].ToObject<string>();
                if (status == null)
                {
                    Console.WriteLine("status was null, how did you even manage to do this");
                }
                else if (status.Equals("error")) {
                    Console.WriteLine("Message status was \"error\" with description:");
                    Console.WriteLine(json["error"]);
                }
            }
        } catch (Exception e)
        {
            Console.WriteLine("Fatal error in scanning for error status");
        }

        if (messageID.Equals("tunnel/send"))
        {
            messageID = json["data"]["data"]["id"].ToObject<string>();
            Console.WriteLine($"TunnelSend => {messageID}");
        }

        //Handle response according to message ID
        switch (messageID)
        {
            default:
                Console.WriteLine("Message ID not recognized from JSON:");
                Console.WriteLine(json);
                break;

            case "session/list":
                vrClient.ListSessions(json);
                break;

            case "tunnel/create":
                string? sessionID = json["data"]["id"].ToObject<string>();

                if (sessionID == null)
                {
                    Console.WriteLine("SessionID was null");
                    break;
                }

                vrClient.tunnelStartup(sessionID);
                break;

            case "scene/get":
                //Console.WriteLine(json);
                vrClient.RemoveObject(json);
                break;
        }
        Console.WriteLine("------------------------------------------------------------Response End");
    }
}