using System.Security.Cryptography;

namespace ClientSide.Encryption;

public class AESHelper
{
	public static byte[] EncryptMessage(string message, byte[] key, byte[] IV) {
		byte[] encrypted;
        
		using AesManaged aes = new AesManaged();
        
		ICryptoTransform encryptor = aes.CreateEncryptor(key, IV);
        
		using MemoryStream ms = new MemoryStream();
		using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
		using(StreamWriter sw = new StreamWriter(cs)) 
			sw.Write(message);  
		encrypted = ms.ToArray();

		return encrypted;  
	}

	public static string? DecryptMessage(byte[] cipherText, byte[] Key, byte[] IV)
	{
		string? decryptedMessage;
        
		using Aes aesAlg = Aes.Create("AesManaged") ?? new AesManaged();
		aesAlg.Key = Key;
		aesAlg.IV = IV;
            
		ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

		using MemoryStream msDecrypt = new MemoryStream(cipherText);
		using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
		using StreamReader srDecrypt = new StreamReader(csDecrypt);
        
		decryptedMessage = srDecrypt.ReadToEnd();

		return decryptedMessage;
	}
}