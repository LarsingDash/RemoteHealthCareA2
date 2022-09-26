namespace ServerApplication.UtilData
{
    public class JsonFolderTest
    {
        JsonFolderTest(string path)
        {
            this.Path = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin", StringComparison.Ordinal)) + path;
        }

        public string Path { get; }

        public override string ToString(){ return Path; }

        public static readonly JsonFolderTest Data = new JsonFolderTest("Data\\");
        public static readonly JsonFolderTest Json = new JsonFolderTest("Json\\");
        public static readonly JsonFolderTest ClientMessages = new JsonFolderTest("Json\\ClientMessages\\");
    }
}