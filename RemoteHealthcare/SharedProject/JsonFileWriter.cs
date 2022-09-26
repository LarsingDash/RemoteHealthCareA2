using Newtonsoft.Json.Linq;

namespace ServerApplication;

public class JsonFileWriter
{
    private static string pathDir = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin", StringComparison.Ordinal)) + "\\Json\\";

    public static void WriteTextToFile(string filename, string text, string path)
    {
        filename =CheckFileName(filename);
        File.WriteAllText((path+filename), text);
        
    }

    public static void WriteObjectToFile(string filename, JObject jObject, string path)
    {
        WriteTextToFile(filename,jObject.ToString(), path);
    }
    
    
    public static string CheckFileName(string fileName)
    {
        if (!fileName.EndsWith(".json"))
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