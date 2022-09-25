namespace ServerApplication.UtilData
{
    public class JsonFolder
    {
        JsonFolder(string path)
        {
            this.Path = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin", StringComparison.Ordinal)) + path;
        }

        public string Path { get; }

        public override string ToString(){ return Path; }

        public static readonly JsonFolder Data = new JsonFolder("Data\\");
        public static readonly JsonFolder Json = new JsonFolder("Json\\");
        public static readonly JsonFolder ClientMessages = new JsonFolder("Json\\ClientMessages\\");
    }
}