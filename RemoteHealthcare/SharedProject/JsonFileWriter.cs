using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using ServerApplication.Encryption;

namespace ServerApplication;

public class JsonFileWriter
{
    private static string pathDir = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin", StringComparison.Ordinal)) + "\\Json\\";
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

    public static void WriteObjectToFile(string filename, JObject jObject, string path)
    {
        WriteTextToFile(filename,jObject.ToString(), path);
    }

    public static void WriteTextToFileEncrypted(string filename, string text, string path)
    {
        var key = EncryptionKeys.GetEncryptKey();
        var iv = EncryptionKeys.GetEncryptIv();

        WriteTextToFile(filename, Util.ByteArrayToString(AesHelper.EncryptMessage(text, key, iv), false),path);
    }


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
    #region Regex functions
    private static string RegexReplace(string source, string pattern, string replacement)
    {
        return Regex.Replace(source,pattern, replacement);
    }

    private static string ReplaceEnd(string source, string value, string replacement)
    {
        return RegexReplace(source, $"{value}$", replacement);
    }

    private static string RemoveEnd(string source, string value)
    {
        return ReplaceEnd(source, value, string.Empty);
    }
    #endregion

}