using ClientSide.Bike;
using ClientSide.VR;

namespace ClientSide
{
    class Program
    {
        /// <summary>
        /// The main function of the program.
        /// </summary>
        /// <param name="args">This is an array of strings that contains the command-line
        /// arguments.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("ClientSide started.");
            // Console.WriteLine($"Machine name: {Environment.MachineName}");
            // Console.WriteLine($"User name: {Environment.UserName}");
            // VRClient vrClient = new VRClient();
            // vrClient.StartConnectionAsync();
            BikeHandler handler = new BikeHandler();
            handler.Subscribe(DataType.Distance, val => Console.WriteLine($"Distance: {val}"));
            handler.Subscribe(DataType.Speed, val => Console.WriteLine($"Speed: {val}"));
            handler.Subscribe(DataType.ElapsedTime, val => Console.WriteLine($"ElapsedTime: {val}"));
            handler.Subscribe(DataType.HeartRate, val => Console.WriteLine($"HeartRate: {val}"));
            //  Console.WriteLine("Ended Main Thread, but the bike is still spinning.");
            Console.Read();

        }
        
        
    }
}
