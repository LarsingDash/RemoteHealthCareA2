using System.Security.Cryptography;

namespace ClientSide.Encryption;

public class RSAHelper
{
	public static byte[]? EncryptMessage(byte[] message, RSAParameters rsaKey, bool doOAEPPadding)
	{
		try
		{
			byte[] encryptedData;
			using (var rsa = new RSACryptoServiceProvider())
			{
				rsa.ImportParameters(rsaKey);
				encryptedData = rsa.Encrypt(message, doOAEPPadding);
			}
			return encryptedData;
		}
		catch (CryptographicException e)
		{
			Console.WriteLine($"Error while trying to encrypt message: \n {e.Message}");
			return null;
		}
	}

	public static byte[]? DecryptMessage(byte[] encryptedMessage, RSAParameters rsaKey, bool doOAEPPadding)
	{
		try
		{
			byte[] decryptedMessage;
			using (var rsa = new RSACryptoServiceProvider())
			{
				rsa.ImportParameters(rsaKey);
				decryptedMessage = rsa.Decrypt(encryptedMessage, doOAEPPadding);
			}
                
			return decryptedMessage;
		}
		catch (CryptographicException e)
		{
			Console.WriteLine($"Error while trying to decrypt message: \n {e.Message}");
			Console.WriteLine($"\n {e.StackTrace}");
			return null;
		}
	}
}