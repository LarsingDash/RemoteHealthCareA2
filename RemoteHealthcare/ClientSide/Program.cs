﻿using ClientSide.Bike;
using ClientSide.VR;
using ServerApplication;

namespace ClientSide
{
    internal static class Program
    {
        /// <summary>
        /// The main function of the program.
        /// </summary>
        /// <param name="args">This is an array of strings that contains the command-line
        /// arguments.</param>

        private static BikeHandler handler;
        
        public static void Main(string[] args)
        {
            JsonFileReader.Initialize(Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin")) + "VR\\Json\\");
            Console.Write("Choose application (1=Bike / VR  2=Client / 3=DoctorGUI): ");
            string option = Console.ReadLine() ?? string.Empty;

            switch (option)
            {
                case "1":
                    Console.WriteLine("BikeClient started");
                    StartBikeClient();
                    
                    Console.WriteLine("VRClient started");
                    var vrClient = new VRClient();
                    vrClient.StartConnectionAsync();
                    break;
                case "2":
                    Console.WriteLine("ServerClient started");
                    StartServerConnection();
                    break;
                case "3":
                    Console.WriteLine("DoctorGUI started");
                    
                    //todo startup application for Doctor GUI
                    break;
                default:
                    Console.WriteLine("No option was chosen");
                    break;

            }

            Console.Read();

        }


        private static void StartBikeClient()
        {
            handler = new BikeHandler();
            // handler.Subscribe(DataType.Distance, val => Console.WriteLine($"Distance: {val}"));
            // handler.Subscribe(DataType.Speed, val => Console.WriteLine($"Speed: {val}"));
            // handler.Subscribe(DataType.ElapsedTime, val => Console.WriteLine($"ElapsedTime: {val}"));
            // handler.Subscribe(DataType.HeartRate, val => Console.WriteLine($"HeartRate: {val}"));
        }

        private static void StartServerConnection()
        {
            //TODO: Add login once finished
            Client client = new Client();
            
        }

        public static Dictionary<DataType, double> GetBikeData()
        {
            return handler.Bike.bikeData;
        }
    }
}
