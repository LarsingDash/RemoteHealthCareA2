using System;

namespace ClientApplication.Util;

public class JsonFolder
{
    JsonFolder(string path)
    {
        this.Path = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("RemoteHealthcare", StringComparison.Ordinal)) + "RemoteHealthcare\\ClientApplication\\" + path;
    }
        
    public string Path { get; }

    /// <summary>
    /// It returns the path of the file.
    /// </summary>
    /// <returns>
    /// The path of the file.
    /// </returns>
    public override string ToString(){ return Path; }
    
    public static readonly JsonFolder Json = new JsonFolder("Json\\");
    public static readonly JsonFolder ClientMessages = new JsonFolder("Json\\ClientMessages\\");
}