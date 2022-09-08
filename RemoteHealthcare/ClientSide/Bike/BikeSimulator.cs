namespace ClientSide.Fiets;

public class BikeSimulator : Bike
{
    private int lastTicks;
    private int ticker;
    private int startedTime;
    public BikeSimulator()
    {
        lastTicks = Environment.TickCount;
        ticker = 0;
        var thread = new Thread(new ThreadStart(Run));
        thread.Start();
    }

    private void Run()
    {
        startedTime = Environment.TickCount;
        while (true)
        {
            var currentTicks = Environment.TickCount;
            ticker++;

            bikeData[DataType.ElapsedTime] = currentTicks - startedTime;
            
            UpdateHeartRate();
            UpdateSpeed();
            UpdateDistance(currentTicks - lastTicks);
            UpdateElapsedTime(currentTicks - startedTime);

            lastTicks = currentTicks;
            
            Thread.Sleep(500);
        }
    }


    private void UpdateHeartRate()
    {
        bikeData[DataType.HeartRate] = 2 * Math.Sin(0.1 * ticker) + 120;
        Console.WriteLine($"HeartRate: {2 * Math.Sin(0.1 * ticker) + 120}");
    }

    private void UpdateSpeed()
    {
        bikeData[DataType.Speed] = (0.7 * Math.Sin(0.05 * ticker) + 22) / 3.6;
        Console.WriteLine($"Speed: {(0.7 * Math.Sin(0.05 * ticker) + 22) / 3.6}");
    }

    private void UpdateDistance(int deltaTime)
    {
        bikeData.TryGetValue(DataType.Distance, out var distance);
        bikeData.TryGetValue(DataType.Speed, out var speed);
        
        bikeData[DataType.Distance] = distance + speed * deltaTime / 1000;
        Console.WriteLine($"Distance: {distance + speed * deltaTime / 1000}");
    }

    private void UpdateElapsedTime(int time)
    {
        bikeData[DataType.ElapsedTime] = (double) (time) / 1000;
        Console.WriteLine($"ElapsedTime: {(double) (time) / 1000}");
    }
}