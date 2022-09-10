using ClientSide.Bike;

namespace ClientSide
{
    class Program
    {
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
            
        }
    }
    
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
