using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SharedProject;

public class AesHelper
{

    /// <summary>
    /// > Encrypts a message using AES, and returns the encrypted message as a byte array
    /// </summary>
    /// <param name="message">The message to be encrypted.</param>
    /// <param name="key">The key used to encrypt the message.</param>
    /// <param name="IV">The initialization vector is a random number that is used to encrypt the first block of text. This
    /// number is then sent in plain text with the encrypted message. The IV is not a secret and can be sent along with the
    /// ciphertext.</param>
    /// <returns>
    /// The encrypted message.
    /// </returns>
    public static byte[] EncryptMessage(string message, byte[] key, byte[] iV) {
        byte[] encrypted;
        
        using Aes aes = Aes.Create("AesManaged")!;
        
        ICryptoTransform encryptor = aes.CreateEncryptor(key, iV);
        
        using MemoryStream ms = new MemoryStream();
        using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using(StreamWriter sw = new StreamWriter(cs)) 
            sw.Write(message);  
        encrypted = ms.ToArray();

        return encrypted;  
    }

    /// <summary>
    /// > Decrypts a message using AES-256 in CBC mode with PKCS7 padding
    /// </summary>
    /// <param name="cipherText">The encrypted message</param>
    /// <param name="Key">The key used to encrypt the message.</param>
    /// <param name="IV">The initialization vector. This is a random number that is used to initialize the encryption
    /// algorithm.</param>
    /// <returns>
    /// A string
    /// </returns>
    public static string? DecryptMessage(byte[] encryptedMessage, byte[] Key, byte[] IV)
    {
        string? decryptedMessage;
        
        using Aes aes = Aes.Create("AesManaged")!;
        aes.Key = Key;
        aes.IV = IV;
            
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream msDecrypt = new MemoryStream(encryptedMessage);
        using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new StreamReader(csDecrypt);
        
        decryptedMessage = srDecrypt.ReadToEnd();

        return decryptedMessage;
    }
}