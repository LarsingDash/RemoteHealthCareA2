namespace ClientSide.Bike.DataPages;

public class DataPage10 : DataPage
{
    
    public DataPage10(BikeHandler handler) : base(handler)
    {
        
    }


    public override void processData(int[] data)
    {
        handler.ChangeData(DataType.Distance, data[3]);
        handler.ChangeData(DataType.ElapsedTime, data[2]);
        handler.ChangeData(DataType.Speed, data[4] + (data[5] << 8));
    }

}