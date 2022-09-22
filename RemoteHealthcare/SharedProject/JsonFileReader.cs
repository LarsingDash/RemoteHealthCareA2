using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SharedProject {


    public static class JsonFileReader {
    private static string pathDir = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin")) + "\\Json\\";

    /// <summary>
    /// It takes a file name and a dictionary of values, and returns a JObject with the values replaced
    /// </summary>
    /// <param name="fileName">The name of the file you want to get the object from.</param>
    /// <param name="values">A dictionary of strings. The key is the string to be replaced, and the value is the string to
    /// replace it with.</param>
    /// <returns>
    /// A JObject
    /// </returns>
    public static JObject GetObject(string fileName, Dictionary<string, string> values, string path)
    {
        fileName = CheckFileName(fileName);
        string ob = JObject.Parse(File.ReadAllText(path + fileName)).ToString();
        foreach (string key in values.Keys)
        {
            ob = ob.Replace(key, values[key]);
        }

        return JObject.Parse(ob);
    }

    public static JObject GetObject(string fileName, Dictionary<string, string> values)
    {
        return GetObject(fileName, values, pathDir);
    }

    /// <summary>
    /// It takes a file name and a dictionary of values, and returns a string representation of the file with the values
    /// replaced
    /// </summary>
    /// <param name="fileName">The name of the file to load.</param>
    /// <param name="values">A dictionary of key/value pairs. The key is the name of the variable in the template, and the
    /// value is the value to replace it with.</param>
    /// <returns>
    /// A string
    /// </returns>
    public static string GetObjectAsString(string fileName, Dictionary<string, string> values)
    {
        return GetObject(fileName, values).ToString();
    }
    
    public static string GetObjectAsString(string fileName, Dictionary<string, string> values, string path)
    {
        return GetObject(fileName, values, path).ToString();
    }

    /// <summary>
    /// If the file name doesn't end with ".json", add it. If the file name starts with a backslash, remove it
    /// </summary>
    /// <param name="fileName">The name of the file you want to save to.</param>
    /// <returns>
    /// The file name with the extension .json
    /// </returns>
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

    public static void Initialize(string jsonFolder)
    {
        pathDir = jsonFolder;
    }

    }
}