namespace ClientSide.Bike;
//TheBikeHandler class is used for the purpose of receiving data from the Bike.
public class BikeHandler
{
    private BikePicker picker = BikePicker.Virtual;
    
    private Dictionary<DataType, List<Action<double>>> observers;
    public Bike Bike { get; }

    public BikeHandler()
    {
        this.observers = new Dictionary<DataType, List<Action<double>>>();
        foreach (DataType val in Enum.GetValues(typeof(DataType)))
        {
            this.observers.Add(val, new List<Action<double>>());
        }
        this.Bike = picker == BikePicker.Virtual ? new BikeSimulator(this) : new BikePhysical(this);
    }
    
    
    /// <summary>
    /// If the observer is not already subscribed to the data type, add it to the list of observers for that data type and
    /// invoke the observer with the current value of the data type
    /// </summary>
    /// <param name="DataType">The type of data you want to subscribe to.</param>
    /// <param name="ob">The observer that is subscribing to the data.</param>
    public void Subscribe(DataType type, Action<double> ob)
    {
        if (!observers[type].Contains(ob)) {
            observers[type].Add(ob);
            ob.Invoke(Bike.bikeData[type]);
        }
    }
    
    /// <summary>
    /// > Unsubscribe an observer from a specific data type
    /// </summary>
    /// <param name="DataType">The type of data you want to subscribe to.</param>
    /// <param name="ob">The observer that is being unsubscribed.</param>
    public void UnSubscribe(DataType type, Action<double> ob)
    {
        if (observers[type].Contains(ob)) {
            observers[type].Remove(ob);
        }
    }

    /// <summary>
    /// It changes the data type and value of the data.
    /// </summary>
    /// <param name="DataType">The type of data you want to change.</param>
    /// <param name="val">The value to change the data to.</param>
    public void ChangeData(DataType type, double val)
    {
        Bike.bikeData[type] = val;
        observers[type].ForEach(ob => ob.Invoke(val));
    }
}

/* An enum that is used to determine which bike to use. */
internal enum BikePicker
{
    Virtual = 1,
    Physical = 2,
}