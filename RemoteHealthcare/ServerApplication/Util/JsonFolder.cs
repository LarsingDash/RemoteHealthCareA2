using System;

namespace ServerApplication;

public class JsonFolder
{
    JsonFolder(string path)
    {
        this.path = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin", StringComparison.Ordinal)) + path;
    }

    public string path { get; }

    public override string ToString(){ return path; }

    public static readonly JsonFolder Data = new JsonFolder("Data\\");
    public static readonly JsonFolder Json = new JsonFolder("Json\\");
    public static readonly JsonFolder ClientMessages = new JsonFolder("Json\\ClientMessages\\");
}