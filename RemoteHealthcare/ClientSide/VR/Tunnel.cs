using Newtonsoft.Json.Linq;

namespace ClientSide.VR;

public class Tunnel
{
    private VRClient vrClient;

    public Tunnel(VRClient vrClient)
    {
        this.vrClient = vrClient;
    }

    //Helper method to send tunnelMessages without having to add the tunnelID
    public void SendTunnelMessage(Dictionary<string, string> values, bool silent = false)
    {
        values.Add("_tunnelID_", vrClient.TunnelID);
        vrClient.SendData(JsonFileReader.GetObjectAsString("SendTunnel", values), silent);
    }

    //Receive response from the server and handle it accordingly to the messageID
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
            if (json.ContainsKey("status"))
            {
                string? status = json["status"].ToObject<string>();
                switch (status)
                {
                    case null:
                        Console.WriteLine("status was null, how did you even manage to do this");
                        break;
                    case "error":
                        Console.WriteLine("Message status was \"error\" with description:");
                        Console.WriteLine(json["error"]);
                        break;
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

                vrClient.TunnelStartup(sessionID);
                break;

            case "scene/get":
                //Console.WriteLine(json);
                vrClient.RemoveObject(json);
                break;
            
            case "scene/node/add":
                var NodeName = json["data"]["data"]["data"]["name"].ToObject<string>();
                var NodeID = json["data"]["data"]["data"]["uuid"].ToObject<string>();
                Console.WriteLine($"Added: {NodeName} with uuid {NodeID}");

                if (NodeName != null && NodeID != null)
                {
                    vrClient.SavedIDs.Add(NodeName, NodeID);
                    if (vrClient.IDWaitList.ContainsKey(NodeName))
                    {
                        Console.WriteLine("Running Action:");
                        vrClient.IDWaitList[NodeName].Invoke(NodeID);
                    }
                }
                break;
        }
        Console.WriteLine("------------------------------------------------------------Response End");
    }
}