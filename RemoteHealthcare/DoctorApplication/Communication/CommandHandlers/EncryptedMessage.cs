using System;
using System.Linq;
using System.Security.Cryptography;
using DoctorApplication.Communication;
using DoctorApplication.Communication.CommandHandlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Encryption;
using Shared.Log;

namespace DoctorApplication.Communication.CommandHandlers;

public class EncryptedMessage : ICommandHandler
{
    private RSA rsa;

    public EncryptedMessage(RSA rsa)
    {
        this.rsa = rsa;
    }
    /// <summary>
    /// It decrypts the message, and then passes it to the DataHandler
    /// </summary>
    /// <param name="client">The client instance</param>
    /// <param name="ob">The message that was received</param>
    public void HandleCommand(Client client, JObject ob)
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
                    JObject json;
                    try
                    {
                        json = JObject.Parse(message);
                       // Logger.LogMessage(LogImportance.Information, $"Got encrypted message: {LogColor.Gray}\n{ob.ToString(Formatting.None)}");
                    } catch(JsonReaderException e)
                    {
                        Logger.LogMessage(LogImportance.Warn, $"Got encrypted message, but message could not be parsed to JSON: {LogColor.Gray}\n{message}", e);
                        return;
                    }
                    client.HandleMessage(json, true);
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
            Logger.LogMessage(LogImportance.Warn, "Error (Unknown Reason)", ex);
        }
    }
}