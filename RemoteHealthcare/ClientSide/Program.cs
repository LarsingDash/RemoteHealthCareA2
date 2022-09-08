using ClientSide.Fiets;

namespace ClientSide
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ClientSide started.");
            Bike bike = new BikeSimulator();
            Console.WriteLine("Started");
        }
    }
}