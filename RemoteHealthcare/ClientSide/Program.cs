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
            Console.WriteLine("Choose application (1=Bike  2=VR)");
            int option = Int32.Parse(s: Console.ReadLine());
            
            switch(option)
            {
                case 1:
                    Console.WriteLine("BikeClient started");
                    StartBikeClient();
                    break;
                case 2:
                    Console.WriteLine("VRClient started");
                    VRClient vrClient = new VRClient();
                    vrClient.StartConnectionAsync();
                    break;

                default:
                    Console.WriteLine("No option was chosen");
                    break;

            }


            // Console.WriteLine($"Machine name: {Environment.MachineName}");
            // Console.WriteLine($"User name: {Environment.UserName}");
            while (true)
            {
                Thread.Sleep(1);
            }
            // BikeHandler handler = new BikeHandler();
            // handler.Subscribe(DataType.Distance, new DefaultObserver("Distance"));
            // handler.Subscribe(DataType.Speed, new DefaultObserver("Speed"));
            // handler.Subscribe(DataType.ElapsedTime, new DefaultObserver("Elapsed Time"));
            // handler.Subscribe(DataType.HeartRate, new DefaultObserver("HeartRate"));
            // Thread.Sleep(5000);
            // Console.WriteLine("Ended Main Thread, but the bike is still spinning.");

        }


        public static void StartBikeClient()
        {
            BikeHandler handler = new BikeHandler();
            handler.Subscribe(DataType.Distance, new DefaultObserver("Distance"));
            handler.Subscribe(DataType.Speed, new DefaultObserver("Speed"));
            handler.Subscribe(DataType.ElapsedTime, new DefaultObserver("Elapsed Time"));
            handler.Subscribe(DataType.HeartRate, new DefaultObserver("HeartRate"));
            Thread.Sleep(5000);
            Console.WriteLine("Ended Main Thread, but the bike is still spinning.");
        }
    }
    
    /* The DefaultObserver class implements the IObserver<double> interface, and it prints the value of the double it
    receives to the console.
    */
    class DefaultObserver : IObserver<double>
    {
        private string s;
        public DefaultObserver(string s)
        {
            this.s = s;
        }
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(double value)
        {
            Console.WriteLine($"{s}: {value}");
        }
    }
}
