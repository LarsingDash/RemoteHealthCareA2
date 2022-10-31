using System;
using System.Threading;
using Shared.Log;

namespace ClientApplication.Bike;
//The BikeSimulator class has the purpose to simulate the bike.
public class BikeSimulator : Bike
{
    private int lastTicks;
    private int ticker;
    private int startedTime;

    private readonly BikeHandler handler;
    
    public readonly bool Bike;
    public readonly bool Heart;
    public BikeSimulator(BikeHandler handler, bool bike = true, bool heart = true, bool state = false)
    {
        this.State = state;
        this.Bike = bike;
        this.Heart = heart;
        lastTicks = Environment.TickCount;
        BikeId = $"SIM {new Random().Next(5000)}";
        ticker = 0;
        this.handler = handler;
        var thread = new Thread(Run);
        thread.IsBackground = true;
        thread.Start();
    }

    private bool running = false;
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
        running = true;
        while (running)
        {
            Thread.Sleep(500);
            if(!State)
                continue;
            var currentTicks = Environment.TickCount;
            ticker++;

            bikeData[DataType.ElapsedTime] = currentTicks - startedTime;
            if (Heart)
            {
                UpdateHeartRate();
            }
            if (Bike)
            {
                UpdateSpeed();
                UpdateDistance(currentTicks - lastTicks);
                UpdateElapsedTime(currentTicks - startedTime);
            }

            lastTicks = currentTicks;
            
        }
    }


    /// <summary>
    /// > The function `UpdateHeartRate` updates the heart rate data by calling the `ChangeData` function of the `handler`
    /// object
    /// </summary>
    private void UpdateHeartRate()
    {
        Logger.LogMessage(LogImportance.DebugHighlight, "Updating HeartRate");
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

    /// <summary>
    /// Reset() is called when the user clicks the reset button. It sets the running variable to false and creates a new
    /// BikeSimulator object
    /// </summary>
    public override void Reset()
    {
        Logger.LogMessage(LogImportance.Fatal, "Resetting");
        running = false;
        App.GetBikeHandlerInstance().Bike = new BikeSimulator(handler, Bike, Heart, State);
    }

    public override void OnStateChange(bool state)
    {
        this.State = state;
        Reset();
    }
}