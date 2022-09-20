namespace ClientSide.Bike.DataPages;

public abstract class DataPage
{
    public BikeHandler handler;
    
    protected DataPage(BikeHandler handler)
    {
        this.handler = handler;
    }
    

    public abstract void ProcessData(int[] data);
}