namespace ClientSide.Bike;
//TheBikeHandler class is used for the purpose of receiving data from the Bike.
public class BikeHandler
{
    private BikePicker picker = BikePicker.Physical;
    
    private Dictionary<DataType, List<IObserver<double>>> observers;
    public Bike bike { get; }

    public BikeHandler()
    {
        this.observers = new Dictionary<DataType, List<IObserver<double>>>();
        foreach (DataType val in Enum.GetValues(typeof(DataType)))
        {
            this.observers.Add(val, new List<IObserver<double>>());
        }
        this.bike = picker == BikePicker.Virtual ? new BikeSimulator(this) : new BikePhysical(this);
    }
    
    
    /// <summary>
    /// If the observer is not already subscribed to the data type, add it to the list of observers for that data type and
    /// send it the current value of the data type
    /// </summary>
    /// <param name="DataType">The type of data you want to subscribe to.</param>
    /// <param name="ob">The observer that is subscribing to the data.</param>
    public void Subscribe(DataType type, IObserver<double> ob)
    {
        if (!observers[type].Contains(ob)) {
            observers[type].Add(ob);
            ob.OnNext(bike.bikeData[type]);
        }
    }
    /// <summary>
    /// If the observer is in the list of observers for the given data type, remove it from the list
    /// </summary>
    /// <param name="DataType">The type of data you want to subscribe to.</param>
    /// <param name="ob">The observer that is being unsubscribed.</param>
    public void UnSubscribe(DataType type, IObserver<double> ob)
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
        bike.bikeData[type] = val;
        observers[type].ForEach(ob => ob.OnNext(val));
    }
}

/* An enum that is used to determine which bike to use. */
internal enum BikePicker
{
    Virtual = 1,
    Physical = 2,
}