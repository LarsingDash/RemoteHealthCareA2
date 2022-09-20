using System.Text;
using Avans.TI.BLE;
using ClientSide.Bike.DataPages;

namespace ClientSide.Bike
{
    public class BikePhysical : Bike
    {
        private BikeHandler handler;
        private Dictionary<int, DataPage> pages;
        private string ID = "01249";
        private BluetoothDevice bikeDevice;
        private BluetoothDevice heartRateDevice;

        public BikePhysical(BikeHandler handler)
        {
            this.handler = handler;
            pages = new Dictionary<int, DataPage>()
            {
                {0x10, new DataPage10(handler)},
            };
            //bikeDevice = new BluetoothDevice($"Tacx Flux {ID}", "6e40fec1-b5a3-f393-e0a9-e50e24dcca9e", "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e", ValueChangedBike);
            //bikeDevice.StartConnection();
            Thread.Sleep(1000);
            heartRateDevice = new BluetoothDevice("Decathlon Dual HR","HeartRate", "HeartRateMeasurement", ValueChangedHeartRate);
            heartRateDevice.StartConnection();
            //Test message
            // NewMessage(DataMessageProtocol.BleBike, "A4 09 4E 05 10 19 6C EE 00 00 FF 24 B6");
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
                     //Console.WriteLine("Data");
                     if (pages.ContainsKey(data[1]))
                     {
                         pages[data[1]].ProcessData(data);
                     }
                     else
                     {
                     }
                     // data.ToList().ForEach(i => Console.WriteLine(i.ToString("X")));

                    break;
                }
                case DataMessageProtocol.HeartRate:
                {
                    handler.ChangeData(DataType.HeartRate, dataPoints[1]);
                    break;
                }
            }
        }
        /// <summary>
        /// A callback function that is called when the BLE device sends a message.
        /// </summary>
        /// <param name="Object">The object that raised the event.</param>
        /// <param name="BLESubscriptionValueChangedEventArgs"></param>
        private void ValueChangedBike(Object sender, BLESubscriptionValueChangedEventArgs e)
        {
            //Console.WriteLine(BitConverter.ToString(e.Data).Replace("-", " "));
            NewMessage(DataMessageProtocol.BleBike, BitConverter.ToString(e.Data).Replace("-", " "));
        }
        /// <summary>
        /// It prints the heart rate to the console.
        /// </summary>
        /// <param name="Object">The object that raised the event.</param>
        /// <param name="BLESubscriptionValueChangedEventArgs"></param>
        private void ValueChangedHeartRate(Object sender, BLESubscriptionValueChangedEventArgs e)
        {
            string mes = BitConverter.ToString(e.Data).Replace("-", " ");
            string[] dataPointsStrings = mes.Split(' ');
            int[] dataPoints = Array.ConvertAll(dataPointsStrings, s => int.Parse(s, System.Globalization.NumberStyles.HexNumber));

            handler.ChangeData(DataType.HeartRate, Convert.ToInt32(dataPoints[1]));
            //Console.WriteLine($"HeartRate: {e.Data}");
        }
    }

/* Creating an enum for the different types of messages that can be sent. */
    public enum DataMessageProtocol
    {
        BleBike = 1,
        HeartRate = 2,
    }
    
}
