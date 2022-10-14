using System;
using System.Collections.Generic;

namespace ClientApplication.ServerConnection.Bike;

public abstract class Bike
{
    public Dictionary<DataType, double> bikeData;
    /* The constructor for the Bike class. */
    protected Bike()
    {
        bikeData = new Dictionary<DataType, double>();
        foreach (DataType u in Enum.GetValues(typeof(DataType)))
        {
            bikeData.Add(u, 0);
        }
    }
}

/* Creating an enumeration of the different types of data that can be sent from the bike. */
public enum DataType : ushort
{
    Speed = 1,
    Distance = 2,
    HeartRate = 3,
    ElapsedTime = 4,
}