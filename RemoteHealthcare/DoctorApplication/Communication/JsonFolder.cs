using System;
using Shared.Log;

namespace ClientApplication.ServerConnection.Communication;

public class JsonFolder
{
    JsonFolder(string path)
    {
        this.Path = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("RemoteHealthcare", StringComparison.Ordinal)) + "RemoteHealthcare\\DoctorApplication\\Communication\\" + path;
        // Logger.LogMessage(LogImportance.Debug, this.Path);
    }
        
    public string Path { get; }

    /// <summary>
    /// It returns the path of the file.
    /// </summary>
    /// <returns>
    /// The path of the file.
    /// </returns>
    public override string ToString(){ return Path; }

    // public static readonly JsonFolder Data = new JsonFolder("Data\\");
    public static readonly JsonFolder Json = new JsonFolder("Json\\");
    public static readonly JsonFolder ClientMessages = new JsonFolder("Json\\ClientMessages\\");
}