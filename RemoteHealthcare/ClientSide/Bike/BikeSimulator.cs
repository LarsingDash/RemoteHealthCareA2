namespace ClientSide.Bike;

public class BikeSimulator : Bike
{
    private int lastTicks;
    private int ticker;
    private int startedTime;

    private BikeHandler handler;
    public BikeSimulator(BikeHandler handler)
    {
        lastTicks = Environment.TickCount;
        ticker = 0;
        var thread = new Thread(Run);
        thread.Start();
        this.handler = handler;
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
        handler.ChangeData(DataType.HeartRate, 2 * Math.Sin(0.1 * ticker) + 120);
        //Console.WriteLine($"HeartRate: {2 * Math.Sin(0.1 * ticker) + 120}");
    }

    private void UpdateSpeed()
    {
        handler.ChangeData(DataType.Speed, (0.7 * Math.Sin(0.05 * ticker) + 22) / 3.6);
        //Console.WriteLine($"Speed: {(0.7 * Math.Sin(0.05 * ticker) + 22) / 3.6}");
    }

    private void UpdateDistance(int deltaTime)
    {
        bikeData.TryGetValue(DataType.Distance, out var distance);
        bikeData.TryGetValue(DataType.Speed, out var speed);
        
        handler.ChangeData(DataType.Distance, distance + speed * deltaTime / 1000);
        //Console.WriteLine($"Distance: {distance + speed * deltaTime / 1000}");
    }

    private void UpdateElapsedTime(int time)
    {
        handler.ChangeData(DataType.ElapsedTime, (double) (time) / 1000);
        //Console.WriteLine($"ElapsedTime: {(double) (time) / 1000}");
    }
}