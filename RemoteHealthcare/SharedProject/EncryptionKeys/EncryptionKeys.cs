using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ServerApplication.Encryption;
using System.IO;

namespace ServerApplication;

public class EncryptionKeys
{
    private static string sharedDir = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("RemoteHealthcare", StringComparison.Ordinal)) + "RemoteHealthcare\\SharedProject" + "\\";

    
    /// <summary>
    /// It reads the AES key from a file and returns it as a byte array
    /// </summary>
    /// <returns>
    /// The encryption key
    /// </returns>
    public static byte[] GetEncryptKey()
    {
        string path = sharedDir + "EncryptionKeys\\AesKey.txt";
        byte[] key = Array.ConvertAll(File.ReadAllText(path).Replace(" ", "").Split(Convert.ToChar(",")), s=> byte.Parse(s));
        return key;
    }
    /// <summary>
    /// It reads the AES IV from a file and returns it as a byte array
    /// </summary>
    /// <returns>
    /// The IV for the AES encryption.
    /// </returns>
    public static byte[] GetEncryptIv()
    {
        string path = sharedDir + "EncryptionKeys\\AesIV.txt";
        byte[] key = Array.ConvertAll(File.ReadAllText(path).Replace(" ", "").Split(Convert.ToChar(",")), s=> byte.Parse(s));
        return key;
    }
}