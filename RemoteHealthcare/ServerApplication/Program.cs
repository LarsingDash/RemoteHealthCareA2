using ServerApplication.Log;
using ServerApplication.UtilData;

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