using System;

namespace ClientApplication.Bike.DataPages;

public class DataPage10 : DataPage
{
    private int[]? prevData;
    
    private int distanceMultiplier;
    private int timeMultiplier;
    
    public DataPage10(BikeHandler handler) : base(handler)
    {
        prevData = null;
    }


    /// <summary>
    /// If the previous data is null, then set the distance, elapsed time, and speed to the current data. Otherwise, if the
    /// previous distance is greater than the current distance (meaning the data has beem reset, max value is 256), then increment the distance multiplier and set the distance
    /// to the current distance plus the distance multiplier times 256. Otherwise, set the distance to the current distance
    /// plus the distance multiplier times 256. Do the same for elapsed time.
    /// Finally, set the speed to the current speed
    /// </summary>
    /// <param name="data">The data received from the device.</param>
    public override void ProcessData(int[] data)
    {
        if (App.GetBikeHandlerInstance().Bike.State == false)
            return;
        if (prevData == null)
        {
            Handler.ChangeData(DataType.Distance, Convert.ToInt32(data[4]));
            Handler.ChangeData(DataType.ElapsedTime, Convert.ToInt32(data[3] / 4));
            Handler.ChangeData(DataType.Speed, (double) Convert.ToInt32(data[5] + (data[6] << 8)) / 1000 * 3.6);
        }
        else
        {
            if (prevData[4] > data[4])
            {
                distanceMultiplier++;
                Handler.ChangeData(DataType.Distance, Convert.ToInt32(data[4]) + distanceMultiplier * 256);
            }
            else
            {
                Handler.ChangeData(DataType.Distance, Convert.ToInt32(data[4]) + distanceMultiplier * 256);
            }

            if (prevData[3] > data[3])
            {
                timeMultiplier++;
                Handler.ChangeData(DataType.ElapsedTime, (double) Convert.ToInt32(data[3])  / 4 + timeMultiplier * 256);
            }
            else
            {
                Handler.ChangeData(DataType.ElapsedTime, (double) Convert.ToInt32(data[3])  / 4 + timeMultiplier * 256);
            }
            Handler.ChangeData(DataType.Speed, (double) Convert.ToInt32(data[5] + (data[6] << 8)) / 1000);
        }
        prevData = data;
    }

}