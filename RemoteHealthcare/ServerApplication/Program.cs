using ServerApplication.Log;

namespace ServerApplication
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Logger.PrintLevel = LogLevel.All;
        
            Server server = new Server();
        
        }
    }
}