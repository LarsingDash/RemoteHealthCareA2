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
    
    public static readonly JsonFolder ServerConnection = new JsonFolder("Json\\ServerConnection\\");
    public static readonly JsonFolder Vr = new JsonFolder("Json\\VR\\");
}