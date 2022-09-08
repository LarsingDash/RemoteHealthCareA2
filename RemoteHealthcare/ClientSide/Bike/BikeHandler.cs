namespace ClientSide.Bike;

public class BikeHandler
{
    private BikePicker picker = BikePicker.Virtual;
    
    private Dictionary<DataType, List<IObserver<double>>> observers;
    private Bike bike;

    public BikeHandler()
    {
        this.observers = new Dictionary<DataType, List<IObserver<double>>>();
        foreach (DataType val in Enum.GetValues(typeof(DataType)))
        {
            this.observers.Add(val, new List<IObserver<double>>());
        }
        this.bike = picker == BikePicker.Virtual ? new BikeSimulator(this) : new BikeSimulator(this); //TODO Add Physical bike
    }
    
    
    public void Subscribe(DataType type, IObserver<double> ob)
    {
        if (!observers[type].Contains(ob)) {
            observers[type].Add(ob);
            ob.OnNext(bike.bikeData[type]);
        }
    }

    public void UnSubscribe(DataType type, IObserver<double> ob)
    {
        if (observers[type].Contains(ob)) {
            observers[type].Remove(ob);
        }
    }

    public void ChangeData(DataType type, double val)
    {
        bike.bikeData[type] = val;
        observers[type].ForEach(ob => ob.OnNext(val));
    }
}

internal enum BikePicker
{
    Virtual = 1,
    Physical = 2,
}