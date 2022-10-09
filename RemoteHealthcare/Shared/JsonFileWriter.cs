using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Shared.Encryption;
using System;
using System.Collections.Generic;
using System.IO;


namespace Shared;

public class JsonFileWriter
{
    private static string pathDir = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin", StringComparison.Ordinal)) + "\\Json\\";
    
    /// <summary>
    /// It takes a filename, text, and path, and writes the text to the file at the path
    /// </summary>
    /// <param name="filename">The name of the file you want to create.</param>
    /// <param name="text">The text you want to write to the file.</param>
    /// <param name="path">The path to the folder where the file will be saved.</param>
    public static void WriteTextToFile(string filename, string text, string path)
    {
        filename =CheckFileName(filename);
        var totalPath = path + filename;
        if (!File.Exists(totalPath))
        {
            (new FileInfo(totalPath)).Directory!.Create();
            File.Create(totalPath).Close();
        }

        File.WriteAllText((totalPath), text);
        
    }

    /// <summary>
    /// WriteObjectToFile(filename, jObject, path)
    /// </summary>
    /// <param name="filename">The name of the file you want to write to.</param>
    /// <param name="JObject">The JObject you want to write to a file.</param>
    /// <param name="path">The path to the folder where you want to save the file.</param>
    public static void WriteObjectToFile(string filename, JObject jObject, string path)
    {
        WriteTextToFile(filename,jObject.ToString(), path);
    }

    /// <summary>
    /// It takes a string, encrypts it, and writes it to a file
    /// </summary>
    /// <param name="filename">The name of the file to be written to.</param>
    /// <param name="text">The text to be encrypted and written to the file.</param>
    /// <param name="path">The path to the file.</param>
    public static void WriteTextToFileEncrypted(string filename, string text, string path)
    {
        var key = EncryptionKeys.GetEncryptKey();
        var iv = EncryptionKeys.GetEncryptIv();

        WriteTextToFile(filename, Util.ByteArrayToString(AesHelper.EncryptMessage(text, key, iv), false),path);
    }


    /// <summary>
    /// It checks to see if the file name ends with ".json" or ".txt" and if it doesn't, it adds ".json" to the end of the
    /// file name
    /// </summary>
    /// <param name="fileName">The name of the file you want to save.</param>
    /// <returns>
    /// The file name with the extension .json or .txt
    /// </returns>
    public static string CheckFileName(string fileName)
    {
        if (!fileName.EndsWith(".json") && !fileName.EndsWith(".txt"))
        {
            fileName += ".json";
        }

        if (fileName.StartsWith("\\"))
        {
            fileName = fileName.Substring(1, fileName.Length);
        }

        return fileName;
    }

}