using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using DoctorApplication.Communication.CommandHandlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Log;
using Formatting = Newtonsoft.Json.Formatting;

namespace DoctorApplication.Communication;

public class Client : DefaultClientConnection
{
    private Dictionary<string, ICommandHandler> commandHandler = new();
    
    public Client()
    {
        commandHandler.Add("public-rsa-key", new RsaKey());
        Init("127.0.0.1", 2460, (json, encrypted) =>
            {
                string extraText = encrypted ? "Encrypted " : "";
               if (commandHandler.ContainsKey(json["id"]!.ToObject<string>()!))
               {
                   if (!json["id"]!.ToObject<string>()!.Equals("encryptedMessage"))
                   {
                       Logger.LogMessage(LogImportance.Information, $"Got {extraText}message from server: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
                   }
                   commandHandler[json["id"]!.ToObject<string>()!].HandleCommand(this, json);
               }
               else
               {
                   Logger.LogMessage(LogImportance.Warn, $"Got {extraText}message from server but no commandHandler found: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
               }
            });
        
        commandHandler.Add("encryptedMessage", new EncryptedMessage(Rsa));

        Thread.Sleep(500);
        
        //Testing...
        var serial = Util.RandomString();
        SendEncryptedData(JsonFileReader.GetObjectAsString("Login", new Dictionary<string, string>()
        {
            {"_serial_", serial},
            {"_type_", "Doctor"},
            {"_username_", "Jasper"},
            {"_password_", "Merijn"}
        }, JsonFolder.Json.Path));
        
        AddSerialCallback(serial, ob =>
        {
           // Logger.LogMessage();
        });
        
        SendEncryptedData(JsonFileReader.GetObjectAsString("ActiveClients", new Dictionary<string, string>()
        {
            //{"_serial_", serial}
        }, JsonFolder.Json.Path));
        
        Thread.Sleep(500);
        SendEncryptedData(JsonFileReader.GetObjectAsString("HistoricClientData", new Dictionary<string, string>()
        {
            {"_name_", "Tes123t1"}
        }, JsonFolder.Json.Path));
        
        SendEncryptedData(JsonFileReader.GetObjectAsString("AllClients", new Dictionary<string, string>(), JsonFolder.Json.Path));
        

    }
}