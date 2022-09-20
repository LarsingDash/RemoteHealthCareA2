namespace ClientSide.Bike.DataPages;

public class DataPage10 : DataPage
{
    private int[]? _prevData;
    
    private int _distanceMultiplier;
    private int _timeMultiplier;
    
    public DataPage10(BikeHandler handler) : base(handler)
    {
        _prevData = null;
    }


    /// <summary>
    /// If the previous data is null, then set the distance, elapsed time, and speed to the current data. Otherwise, if the
    /// previous distance is greater than the current distance, then increment the distance multiplier and set the distance
    /// to the current distance plus the distance multiplier times 256. Otherwise, set the distance to the current distance
    /// plus the distance multiplier times 256. If the previous elapsed time is greater than the current elapsed time, then
    /// increment the elapsed time multiplier and set the elapsed time to the current elapsed time plus the elapsed time
    /// multiplier times 256. Otherwise, set the elapsed time to the current elapsed time plus the elapsed time multiplier
    /// times 256. Finally, set the speed to the current speed
    /// </summary>
    /// <param name="data">The data received from the device.</param>
    public override void ProcessData(int[] data)
    {
        //Console.WriteLine(String.Join(" ", new List<int>(data).ConvertAll(i => i.ToString("X")).ToArray()));
        //Console.WriteLine(" ");

        if (_prevData == null)
        {
            handler.ChangeData(DataType.Distance, Convert.ToInt32(data[4]));
            handler.ChangeData(DataType.ElapsedTime, Convert.ToInt32(data[3] / 4));
            handler.ChangeData(DataType.Speed, (double) Convert.ToInt32(data[5] + (data[6] << 8)) / 1000 * 3.6);
        }
        else
        {
            if (_prevData[4] > data[4])
            {
                _distanceMultiplier++;
                handler.ChangeData(DataType.Distance, Convert.ToInt32(data[4]) + _distanceMultiplier * 256);
            }
            else
            {
                handler.ChangeData(DataType.Distance, Convert.ToInt32(data[4]) + _distanceMultiplier * 256);
            }

            if (_prevData[3] > data[3])
            {
                _timeMultiplier++;
                handler.ChangeData(DataType.ElapsedTime, (double) Convert.ToInt32(data[3])  / 4 + _timeMultiplier * 256);
            }
            else
            {
                handler.ChangeData(DataType.ElapsedTime, (double) Convert.ToInt32(data[3])  / 4 + _timeMultiplier * 256);
            }
            handler.ChangeData(DataType.Speed, (double) Convert.ToInt32(data[5] + (data[6] << 8)) / 1000 * 3.6);
        }

        _prevData = data;
    }

}