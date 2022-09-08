namespace ClientSide.Fiets;

public abstract class Bike
{
    public Dictionary<DataType, double> bikeData;
    public Bike()
    {
        bikeData = new Dictionary<DataType, double>();
        foreach (DataType u in Enum.GetValues(typeof(DataType)))
        {
            bikeData.Add(u, 0);
        }
    }
}

public enum DataType : ushort
{
    Speed = 1,
    Distance = 2,
    HeartRate = 3,
    ElapsedTime = 4,
}