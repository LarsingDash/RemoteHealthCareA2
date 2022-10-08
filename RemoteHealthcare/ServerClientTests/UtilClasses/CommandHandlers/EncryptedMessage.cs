using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Encryption;
using Shared.Log;

namespace ServerClientTests.UtilClasses.CommandHandlers;

public class EncryptedMessage : ICommandHandler
{
    /// <summary>
    /// It decrypts the message, and then handles it
    /// </summary>
    /// <param name="DefaultClientConnection">The connection to the client</param>
    /// <param name="JObject">The JObject that was received from the client.</param>
    public void HandleCommand(DefaultClientConnection client, JObject ob)
    {
        try
        {
            var keyCrypted = ob["aes-keys"]!.Value<JArray>("Key")!.Values<byte>().ToArray();
            var iVCrypted = ob["aes-keys"]!.Value<JArray>("IV")!.Values<byte>().ToArray();
                        
            var messageCrypted = ob.Value<JArray>("aes-data")!.Values<byte>().ToArray();  
                        
            var key = RsaHelper.DecryptMessage(keyCrypted, client.Rsa.ExportParameters(true), false);
            var iV = RsaHelper.DecryptMessage(iVCrypted, client.Rsa.ExportParameters(true), false);
            if (key != null && iV != null)
            {
                var message = AesHelper.DecryptMessage(messageCrypted, key, iV);
                if (message != null)
                {
                    try
                    {
                        JObject json = JObject.Parse(message);
                        //Logger.LogMessage(LogImportance.Information, $"Got encrypted message: {LogColor.Gray}\n{ob.ToString(Formatting.None)}");
                        client.HandleMessage(json);
                    } catch(JsonReaderException e)
                    {
                        Logger.LogMessage(LogImportance.Warn, $"Got encrypted message, but message could not be parsed to JSON: {LogColor.Gray}\n{message}", e);
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
            Logger.LogMessage(LogImportance.Warn, "Error (Unknown Reason)", ex);
        }
    }
}