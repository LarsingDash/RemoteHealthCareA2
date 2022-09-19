using System.Threading.Channels;
using System.Xml;
using Avans.TI.BLE;
using ClientSide.Bike.DataPages;

namespace ClientSide.Bike
{

    public class BikePhysical : Bike
    {
        private BikeHandler handler;
        private Dictionary<int, DataPage> pages;

        public static int ID = 01249;


        public BikePhysical(BikeHandler handler)
        {
            this.handler = handler;
            pages = new Dictionary<int, DataPage>()
        {
            {0x10, new DataPage10(handler)},

        };

            //Discardable new thread to connect to bike
            new Thread(ConnectToBike).Start();
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

                        if (current != dataPoints[dataPoints.Length - 1])
                        {
                            Console.WriteLine("Received Message is Invalid.");
                            return;
                        }

                        int[] data = new int[msgLength];
                        for (int i = 3; i < 3 + msgLength; i++)
                        {
                            data[i - 3] = dataPoints[i];
                        }
                        //Console.WriteLine("Data");
                        //data.ToList().ForEach(i => Console.WriteLine(i.ToString("X")));

                        break;
                    }
                case DataMessageProtocol.HeartRate:
                    {
                        handler.ChangeData(DataType.HeartRate, dataPoints[1]);
                        break;
                    }
            }
        }

        private static async void ConnectToBike()
        {
            Console.WriteLine("Starting Connection");
            Console.WriteLine(Thread.CurrentThread.Name);
            int errorCode = 0;
            BLE bleBike = new BLE();
            BLE bleHeart = new BLE();
            Thread.Sleep(1000);

            //Get devices and services
            List<String> bleBikeList = bleBike.ListDevices();
            Console.WriteLine("Devices found: ");
            foreach (var name in bleBikeList)
            {
                Console.WriteLine($"Device: {name}");
            }
            var services = bleBike.GetServices;

            //Attempt connection with Bike
            errorCode = errorCode = await bleBike.OpenDevice($"Tacx Flux {ID}");

            //Set service
            errorCode = await bleBike.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");

            //Subscribe Bike
            bleBike.SubscriptionValueChanged += OnValueChanged;
            errorCode = await bleBike.SubscribeToCharacteristic("6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");

            //Subscribe Heart
            errorCode = await bleHeart.OpenDevice("Decathlon Dual HR");
            await bleHeart.SetService("HeartRate");
            bleHeart.SubscriptionValueChanged += OnValueChanged;

            Console.WriteLine($"ErrorCode: {errorCode}");
            Console.Read();
        }

        private static void OnValueChanged(Object sender, BLESubscriptionValueChangedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }

    /* Creating an enum for the different types of messages that can be sent. */
    public enum DataMessageProtocol
    {
        BleBike = 1,
        HeartRate = 2,
    }
}   
