namespace ClientSide.Bike.DataPages;

public abstract class DataPage
{
    protected readonly BikeHandler Handler;
    
    protected DataPage(BikeHandler handler)
    {
        Handler = handler;
    }
    

    public abstract void ProcessData(int[] data);
}