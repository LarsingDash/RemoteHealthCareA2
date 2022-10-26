using System;
using System.Threading;

namespace ClientApplication.Bike;
//The BikeSimulator class has the purpose to simulate the bike.
public class BikeSimulator : Bike
{
    private int lastTicks;
    private int ticker;
    private int startedTime;

    private readonly BikeHandler handler;
    
    private bool bike;
    private bool heart;
    public BikeSimulator(BikeHandler handler, bool bike = true, bool heart = true)
    {
        
        this.bike = bike;
        this.heart = heart;
        lastTicks = Environment.TickCount;
        BikeId = $"SIM {new Random().Next(5000)}";
        ticker = 0;
        var thread = new Thread(Run);
        thread.Start();
        this.handler = handler;
    }

    /// <summary>
    /// The Run function is a while loop that runs forever. Every time it updates the values:
    /// - HeartRate
    /// - Speed
    /// - Distance
    /// - ElapsedTime
    /// </summary>
    private void Run()
    {
        startedTime = Environment.TickCount;
        while (true)
        {
            var currentTicks = Environment.TickCount;
            ticker++;

            bikeData[DataType.ElapsedTime] = currentTicks - startedTime;
            if (heart)
            {
                UpdateHeartRate();
            }
            if (bike)
            {
                UpdateSpeed();
                UpdateDistance(currentTicks - lastTicks);
                UpdateElapsedTime(currentTicks - startedTime);
            }

            lastTicks = currentTicks;
            
            Thread.Sleep(500);
        }
    }


    /// <summary>
    /// > The function `UpdateHeartRate` updates the heart rate data by calling the `ChangeData` function of the `handler`
    /// object
    /// </summary>
    private void UpdateHeartRate()
    {
        handler.ChangeData(DataType.HeartRate, 2 * Math.Sin(0.1 * ticker) + 120);
    }

    /// <summary>
    /// > The function `UpdateSpeed()` updates the speed of the bike by using the `ChangeData()` function of the `handler`
    /// object
    /// </summary>
    private void UpdateSpeed()
    {
        handler.ChangeData(DataType.Speed, (0.7 * Math.Sin(0.05 * ticker) + 22) / 3.6);
    }

    /// <summary>
    /// > Update the distance traveled by the bike by adding the distance traveled in the last second to the total distance
    /// traveled
    /// </summary>
    /// <param name="deltaTime">the time in milliseconds since the last update</param>
    private void UpdateDistance(int deltaTime)
    {
        bikeData.TryGetValue(DataType.Distance, out var distance);
        bikeData.TryGetValue(DataType.Speed, out var speed);
        
        handler.ChangeData(DataType.Distance, distance + speed * deltaTime / 1000);
    }

    /// <summary>
    /// It updates the elapsed time.
    /// </summary>
    /// <param name="time">The time in milliseconds since the start of the game.</param>
    private void UpdateElapsedTime(int time)
    {
        handler.ChangeData(DataType.ElapsedTime, (double) (time) / 1000);
    }

    public override void SetResistanceAsync(int ressistance)
    {
        // Do nothing, is simulator
    }
}