using SharedProject;

namespace ServerApplication
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            JsonFileReader.Initialize(JsonFolder.Json.ToString());
            
            Server server = new Server();
        }
    }
}