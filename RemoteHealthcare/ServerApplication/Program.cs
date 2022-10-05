using Shared.Log;

namespace ServerApplication
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Logger.PrintLevel = LogLevel.All;

            Server server = new Server();

            Console.ReadKey();

        }
    }
}