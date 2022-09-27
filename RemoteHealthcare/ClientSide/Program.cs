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
            Console.Write("Choose application (1=Bike  2=VR  3=Client): ");
            string option = Console.ReadLine() ?? string.Empty;

            switch (option)
            {
                case "1":
                    Console.WriteLine("BikeClient started");
                    StartBikeClient();
                    break;
                case "2":
                    Console.WriteLine("VRClient started");
                    VRClient vrClient = new VRClient();
                    vrClient.StartConnectionAsync();
                    break;
                case "3":
                    Console.WriteLine("ServerClient started");
                    StartServerConnection();
                    break;
                default:
                    Console.WriteLine("No option was chosen");
                    break;

            }


            // Console.WriteLine($"Machine name: {Environment.MachineName}");
            // Console.WriteLine($"User name: {Environment.UserName}");
            Console.Read();

        }


        public static void StartBikeClient()
        {
            BikeHandler handler = new BikeHandler();
            handler.Subscribe(DataType.Distance, val => Console.WriteLine($"Distance: {val}"));
            handler.Subscribe(DataType.Speed, val => Console.WriteLine($"Speed: {val}"));
            handler.Subscribe(DataType.ElapsedTime, val => Console.WriteLine($"ElapsedTime: {val}"));
            handler.Subscribe(DataType.HeartRate, val => Console.WriteLine($"HeartRate: {val}"));
        }

        private static void StartServerConnection()
        {
            //TODO: Add login once finished
            Client client = new Client();
            
        }
    }
}
