using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography;
using ClientSide.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerApplication;
using SharedProject;
using SharedProject.Log;

namespace ServerSide.CommandHandlers;

public class EncryptedMessage : ICommandHandler
{
    private RSA rsa;

    public EncryptedMessage(RSA rsa)
    {
        this.rsa = rsa;
    }
    public void HandleMessage(Server server, ClientData data, JObject ob)
    {
        try
        {
            var keyCrypted = ob["aes-keys"]!.Value<JArray>("Key")!.Values<byte>().ToArray();
            var iVCrypted = ob["aes-keys"]!.Value<JArray>("IV")!.Values<byte>().ToArray();
                        
            var messageCrypted = ob.Value<JArray>("aes-data")!.Values<byte>().ToArray();  
                        
            var key = RsaHelper.DecryptMessage(keyCrypted, rsa.ExportParameters(true), false);
            var iV = RsaHelper.DecryptMessage(iVCrypted, rsa.ExportParameters(true), false);
            if (key != null && iV != null)
            {
                var message = AesHelper.DecryptMessage(messageCrypted, key, iV);
                if (message != null)
                {
                    try
                    {
                        JObject json = JObject.Parse(message);
                        Logger.LogMessage(LogImportance.Information, $"Got encrypted message: {LogColor.Gray}\n{json.ToString(Formatting.None)}");
                        server.OnMessage(data, json);
                    } catch(JsonReaderException e)
                    {
                        Logger.LogMessage(LogImportance.Error, $"Got encrypted message, but message could not be parsed to JSON: {LogColor.Gray}\n{message}", e);
                    }
                }
                else
                {
                    Logger.LogMessage(LogImportance.Warn, "Got encrypted message, but message is null");
                }
            }
            else
            {
                Logger.LogMessage(LogImportance.Warn, "Received encrypted Message, but could not decrypt Aes-keys. Did sender use correct Public key of RSA");
            }
        }
        catch(Exception ex)
        {
            Logger.LogMessage(LogImportance.Error, "Error (Unknown Reason)", ex);
        }
    }
}