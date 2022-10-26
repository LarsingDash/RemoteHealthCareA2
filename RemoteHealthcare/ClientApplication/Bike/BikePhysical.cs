using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avans.TI.BLE;
using ClientApplication.Bike.DataPages;
using Shared.Log;

namespace ClientApplication.Bike
{
    public class BikePhysical : Bike
    {
        // Change this to the last 5 digits of the serial number of the bike.
        private string id = "24517";
        
        private BikeHandler handler;
        private Dictionary<int, DataPage> pages;
        private BluetoothDevice? bikeDevice;
        private BluetoothDevice heartRateDevice;

       

        public BikePhysical(BikeHandler handler)
        {

            this.handler = handler;
            pages = new Dictionary<int, DataPage>()
            {
                {0x10, new DataPage10(handler)},
            };
            //StartConnection();
            // Test message
            // NewMessage(DataMessageProtocol.BleBike, "A4 09 4E 05 10 19 6C EE 00 00 FF 24 B6");
        }

        public async Task StartConnection()
        {
            bikeDevice = new BluetoothDevice($"Tacx Flux {id}", "6e40fec1-b5a3-f393-e0a9-e50e24dcca9e", "6e40fec2-b5a3-f393-e0a9-e50e24dcca9e", ValueChangedBike);
            await bikeDevice.StartConnection();
            await Task.Delay(5000);
            heartRateDevice = new BluetoothDevice("Decathlon Dual HR","HeartRate", "HeartRateMeasurement", ValueChangedHeartRate);
            await heartRateDevice.StartConnection();
            if (!bikeDevice.Connected || !heartRateDevice.Connected)
            {
                if(!bikeDevice.Connected && !heartRateDevice.Connected)
                {
                   handler.Bike = new BikeSimulator(handler, !bikeDevice.Connected, !heartRateDevice.Connected);
                } else
                {
                    new BikeSimulator(handler, !bikeDevice.Connected, !heartRateDevice.Connected);
                }
                Logger.LogMessage(LogImportance.Information, $"Switching to Bike Simulator for Bike: {!bikeDevice.Connected}, Heart: {!heartRateDevice.Connected} ");
            }
            else
            {
                BikeId = $"Tacx Flux {id}";
                SetResistanceAsync(1);
                Thread.Sleep(10000);
                SetResistanceAsync(0);
            }
        }

        public override void SetResistanceAsync(int resistance)
        {
            if (bikeDevice == null)
                return;
            byte[] output = new byte[13];
            output[0] = 0x4A; //sync byte
            output[1] = 0x09; //Message Lenght
            output[2] = 0x4E; //Message type
            output[3] = 0x05; //Message type
            output[4] = 0x30; //Datatype
            output[11] = (byte)resistance;
            byte checksum = output[0];
            for (int i = 1; i < 12; i++)
            {
                checksum ^= output[i];
            }
            output[12] = checksum;
            bikeDevice.ble.WriteCharacteristic("6e40fec3-b5a3-f393-e0a9-e50e24dcca9e", output);
        }
        

        /// <summary>
        /// The function takes in a message and a protocol, and then parses the message based on the protocol
        /// </summary>
        /// <param name="DataMessageProtocol">This is the type of message that is being sent.</param>
        /// <param name="mes">The message received from the device.</param>
        private void NewMessage(DataMessageProtocol prot, string mes)
        {
            string[] dataPointsStrings = mes.Split(' ');
            int[] dataPoints = Array.ConvertAll(dataPointsStrings, s => int.Parse(s, System.Globalization.NumberStyles.HexNumber));

            switch (prot)
            {
                case DataMessageProtocol.BleBike:
                {
                    int msgId = dataPoints[2];
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
                    if (pages.ContainsKey(data[1]))
                     {
                         pages[data[1]].ProcessData(data);
                     }
                     else
                     {
                         // page not found...
                     }
                     break;
                }
                case DataMessageProtocol.HeartRate:
                {
                    handler.ChangeData(DataType.HeartRate, dataPoints[1]);
                    break;
                }
                default:
                    Console.WriteLine("DataProtocolMessage not found, could not parse data.");
                    break;
            }
        }
        /// <summary>
        /// A callback function that is called when the BLE device sends a message.
        /// </summary>
        /// <param name="Object">The object that raised the event.</param>
        /// <param name="BLESubscriptionValueChangedEventArgs"></param>
        private void ValueChangedBike(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            NewMessage(DataMessageProtocol.BleBike, BitConverter.ToString(e.Data).Replace("-", " "));
        }
        /// <summary>
        /// It prints the heart rate to the console.
        /// </summary>
        /// <param name="Object">The object that raised the event.</param>
        /// <param name="BLESubscriptionValueChangedEventArgs"></param>
        private void ValueChangedHeartRate(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            NewMessage(DataMessageProtocol.HeartRate, BitConverter.ToString(e.Data).Replace("-", " "));
            
            // Old way, now using NewMessage. Needs to be tested.
            // string mes = BitConverter.ToString(e.Data).Replace("-", " ");
            // string[] dataPointsStrings = mes.Split(' ');
            // int[] dataPoints = Array.ConvertAll(dataPointsStrings, s => int.Parse(s, System.Globalization.NumberStyles.HexNumber));
            // handler.ChangeData(DataType.HeartRate, Convert.ToInt32(dataPoints[1]));
        }

        public void Close()
        {
            bikeDevice.ble.CloseDevice();
            heartRateDevice.ble.CloseDevice();
        }
    }

/* Creating an enum for the different types of messages that can be sent. */
    public enum DataMessageProtocol
    {
        BleBike = 1,
        HeartRate = 2,
    }
    
    
}
