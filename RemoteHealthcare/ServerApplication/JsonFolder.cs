using System;

namespace ServerApplication;

public class JsonFolder
{
    JsonFolder(string path)
    {
        this.path = path;
    }

    public string path { get; }

    public override string ToString(){ return path; }

    public static readonly JsonFolder Data = new JsonFolder("\\Data\\");
    public static readonly JsonFolder Json = new JsonFolder("\\Json\\");
}