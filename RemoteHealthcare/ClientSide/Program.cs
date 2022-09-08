using ClientSide.Bike;

namespace ClientSide
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ClientSide started.");
            BikeHandler handler = new BikeHandler();
            DistanceObserver obs = new DistanceObserver();
            handler.Subscribe(DataType.Distance, obs);
            Thread.Sleep(5000);
            handler.UnSubscribe(DataType.Distance, obs);
            Console.WriteLine("Ended Main Thread, but the bike is still spinning.");
        }
    }

    class DistanceObserver : IObserver<double>
    {
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
            Console.WriteLine($"Distance: {value}");
        }
    }
}
