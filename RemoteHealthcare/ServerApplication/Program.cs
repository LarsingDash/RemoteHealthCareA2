using Shared.Log;

namespace ServerApplication
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Logger.PrintLevel = LogLevel.All;

            Server server = new Server();
            // JsonFileWriter.WriteTextToFileEncrypted("Tes123t1\\Te3122st.txt","TestBericht",JsonFolder.Data.Path);

            Console.ReadKey();

        }
    }
}