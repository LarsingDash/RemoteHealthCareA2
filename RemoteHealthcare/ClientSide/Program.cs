using ClientSide.Bike;

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
            BikeHandler handler = new BikeHandler();
            handler.Subscribe(DataType.Distance, new DefaultObserver("Distance"));
            handler.Subscribe(DataType.Speed, new DefaultObserver("Speed"));
            handler.Subscribe(DataType.ElapsedTime, new DefaultObserver("Elapsed Time"));
            handler.Subscribe(DataType.HeartRate, new DefaultObserver("HeartRate"));
            Thread.Sleep(5000);
            Console.WriteLine("Ended Main Thread, but the bike is still spinning.");
            Console.Read();            
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
