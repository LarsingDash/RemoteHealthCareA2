namespace ClientSide.Bike.DataPages;

public abstract class DataPage
{
    public BikeHandler handler;
    
    protected DataPage(BikeHandler handler)
    {
        this.handler = handler;
    }
    

    public abstract void processData(int[] data);
}