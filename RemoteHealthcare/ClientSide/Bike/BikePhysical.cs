using System.Threading.Channels;
using System.Xml;
using ClientSide.Bike.DataPages;

namespace ClientSide.Bike;

public class BikePhysical : Bike
{
    private BikeHandler handler;
    private Dictionary<int, DataPage> pages;
    
    public BikePhysical(BikeHandler handler)
    {
        this.handler = handler;
        pages = new Dictionary<int, DataPage>()
        {
            {0x10, new DataPage10(handler)},
            
        };
        //Test message
        NewMessage(DataMessageProtocol.BleBike, "A4 09 4E 05 10 19 6C EE 00 00 FF 24 B6");
    }

    /// <summary>
    /// The function takes in a message and a protocol, and then parses the message based on the protocol
    /// </summary>
    /// <param name="DataMessageProtocol">This is the type of message that is being sent.</param>
    /// <param name="mes">The message received from the device.</param>
    public void NewMessage(DataMessageProtocol prot, string mes)
    {
        string[] dataPointsStrings = mes.Split(' ');
        int[] dataPoints = Array.ConvertAll(dataPointsStrings, s => int.Parse(s, System.Globalization.NumberStyles.HexNumber));

        switch (prot)
        
        {
            case DataMessageProtocol.BleBike:
            {
                int msgID = dataPoints[2];
                int msgLength = dataPoints[1];
                int current = 0;
                for (int i = 0; i < dataPoints.Length - 1; i++)
                {
                    current ^= dataPoints[i];
                }

                if (current != dataPoints[dataPoints.Length-1])
                {
                    Console.WriteLine("Received Message is Invalid.");
                    return;
                }

                int[] data = new int[msgLength];
                for (int i = 3; i < 3 + msgLength; i++)
                {
                    data[i-3] = dataPoints[i];
                }
                Console.WriteLine("Data");
                data.ToList().ForEach(i => Console.WriteLine(i.ToString("X")));

                break;
            }
            case DataMessageProtocol.HeartRate:
            {
                handler.ChangeData(DataType.HeartRate, dataPoints[1]);
                break;
            }
        }
    }

}

/* Creating an enum for the different types of messages that can be sent. */
public enum DataMessageProtocol
{
    BleBike = 1,
    HeartRate = 2,
}
