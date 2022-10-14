using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using Shared;

namespace ClientApplication.ServerConnection.VR;

/// <summary>
/// Manages the communication between client and server AKA tunnel
/// </summary>
public class TunnelOld
{
    private VrClient vrClient;

    public TunnelOld(VrClient vrClient)
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
    
    /// <summary>
    /// This method handles the response send from the VR server
    /// and invokes methods/actions in the dictionaries of VRClient (see: VRClient)
    /// After confirming the JSON object is valid, the method looks at the ids and/or the sub ids in json[data][data][id]
    /// Based on the id, the method will call an Action that has that id as a key (see: dictionary fields in VRClient)
    /// todo: replace with observer pattern
    /// </summary>
    /// <param name="client">VRClient that is connected with the VR engine</param>
    /// <param name="json">The response message of the VR engine</param>
    public void HandleResponse(VrClient client, JObject json)
    {
        // Console.WriteLine("------------------------------------------------------------Response Start");
        string? messageID;
        
        //Attempt to find the messageID and handle any exceptions
        try
        {
            messageID = json["id"].ToObject<string>();
            // Console.WriteLine($"Message ID: {messageID}");
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
            // Console.WriteLine($"TunnelSend => {messageID}");
        }

        //Handle response according to message ID
        switch (messageID)
        {
            default:
                // Console.WriteLine("Message ID not recognized from JSON:");
                // Console.WriteLine(json);
                break;

            case "session/list":
                vrClient.ListSessions(json);
                break;

            case "tunnel/create":
                string? sessionID = json["data"]?["id"]?.ToObject<string>();

                if (sessionID == null)
                {
                    Console.WriteLine("SessionID was null");
                    break;
                }

                vrClient.TunnelStartup(sessionID);
                break;

            case "scene/get":
                // Console.WriteLine(json);
                vrClient.RemoveObject(json);
                break;
            
            case "scene/node/add":
                try
                {
                    var nodeName = json["data"]["data"]["data"]["name"].ToObject<string>();
                    var nodeId = json["data"]["data"]["data"]["uuid"].ToObject<string>();
                    // Console.WriteLine($"Added: {nodeName} with uuid {nodeId}");

                    if (nodeName != null && nodeId != null)
                    {
                        vrClient.SavedIDs.Add(nodeName, nodeId);
                        if (vrClient.IDWaitList.ContainsKey(nodeName))
                        {
                            // Console.WriteLine("Running Action:");
                            vrClient.IDWaitList[nodeName].Invoke(nodeId);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error:\nMessage: {e.Message}\nSource: {e.Source}\nStacktrace: {e.StackTrace}");
                    Console.WriteLine(json);
                }

                break;
            
            case "scene/terrain/add":
                vrClient.worldGen.PathGen();
                break;
            
            case "route/add":
                var routeId = json["data"]["data"]["data"]["uuid"].ToObject<string>();
                // Console.WriteLine($"Added: route with uuid {routeId}");
                string routeName = "route";
                if (routeId != null)
                {
                    vrClient.SavedIDs.Add(routeName, routeId);
                    if (vrClient.IDWaitList.ContainsKey(routeName))
                    {
                        // Console.WriteLine("Running Action:");
                        vrClient.IDWaitList[routeName].Invoke(routeId);
                    }
                }

                vrClient.bikeController.AnimateBike();
                break;
            
            case "scene/node/find":
                try
                {
                    // Console.WriteLine(json);
                    var foundName = json["data"]["data"]["data"][0]["name"].ToObject<string>();
                    var foundID = json["data"]["data"]["data"][0]["uuid"].ToObject<string>();
                    // Console.WriteLine($"Found: {foundName} with uuid {foundID}");

                    if (foundName != null && foundID != null)
                    {
                        if (vrClient.IDSearchList.ContainsKey(foundName))
                        {
                            vrClient.IDSearchList[foundName].Invoke(foundID);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                break;
        }
        // Console.WriteLine("------------------------------------------------------------Response End");
    }
}