using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Shared.Log;

namespace DoctorApplication.MVVM.Model
{
    public class SessionModel : INotifyPropertyChanged
    {
        public string sessionName { get; set; }

        private DateTime startTime;
        public DateTime StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }

        private DateTime endTime;
        public DateTime EndTime
        {
            get
            {
                return endTime;
            }
            set
            {
                endTime = value;
                OnPropertyChanged(nameof(EndTime));
            }
        }

        public int sessionint = 0;

        string timeElapsed;

        public string TimeElapsed
        {
            get
            {
                return timeElapsed;
            }
            set
            {
                timeElapsed = value;
                OnPropertyChanged(nameof(TimeElapsed));
            }
        }

        private List<double> distance;
        public List<double> Distance
        {
            get
            {
                return distance;
            }
            set
            {
                distance = value;
                OnPropertyChanged(nameof(Distance));
            }
        }
        private List<double> speed;
        public List<double> Speed
        {
            get { return speed; }
            set
            {
                speed = value;
                OnPropertyChanged(nameof(Speed));
            }
        }
        private List<double> heartRate;
        public List<double> HeartRate
        {
            get { return heartRate; }
            set
            {
                heartRate = value;
                OnPropertyChanged(nameof(HeartRate));
            }
        }

        private double currentDistance;
        public double CurrentDistance
        {
            get
            {
                if (currentDistance != 0) { return currentDistance; }
                else return 0;
            }
            set
            {
                currentDistance = value;
                OnPropertyChanged(nameof(CurrentDistance));
            }
        }

        private double currentSpeed;
        public double CurrentSpeed
        {
            get
            {
                if (currentSpeed != 0) { return currentSpeed; }
                else return 0;
            }
            set
            {
                currentSpeed = value;
                OnPropertyChanged(nameof(CurrentSpeed));
            }
        }
        private double currentRate;
        public double CurrentRate
        {
            get
            {
                if (currentRate != 0) { return currentRate; }
                else return 0;
            }
            set
            {
                currentRate = value;
                OnPropertyChanged(nameof(CurrentRate));
            }
        }

        private double topSpeed;
        public double TopSpeed
        {
            get
            {
                if (topSpeed != 0) { return topSpeed; }
                else return 0;
            }
            set
            {
                topSpeed = value;
                OnPropertyChanged(nameof(TopSpeed));
            }
        }

        private double averageSpeed;
        public double AverageSpeed
        {
            get { return averageSpeed; }
            set
            {
                averageSpeed = value;
                OnPropertyChanged(nameof(AverageSpeed));
            }
        }


       
        //statistic data heartmonitor

        private double lowestRate;
        public double LowestRate
        {
            get
            {
                if (lowestRate != 0) { return lowestRate; }
                else return 0;
            }
            set
            {
                lowestRate = value;
                OnPropertyChanged(nameof(LowestRate));
            }
        }


        private double averageRate;
        public double AverageRate
        {
            get
            {
                if (averageRate != 0) { return averageRate; }
                else return 0;
            }
            set
            {
                averageRate = value;
                OnPropertyChanged(nameof(AverageRate));
            }
        }
        private double highestRate;
        public double HighestRate
        {
            get
            {
                double highest = 0;
                foreach (double value in HeartRate)
                {
                    if (value > highest)
                    {
                        highest = value;
                    }
                }
                return highest;
            }
            set
            {
                highestRate = value;
                OnPropertyChanged(nameof(highestRate));
            }
        }
        /// <summary>
        /// It takes a list of doubles, and if the list has at least one item, it calculates the average, maximum, and
        /// current speed, and the current distance
        /// </summary>
        /// <param name="speedValues">A list of doubles that contains the speed values for the current session.</param>
        public void UpdateSpeedValues(List<double> speedValues)
        {
            if (speedValues.Count > 0)
            {
                AverageSpeed = Math.Round(speedValues.Average() * 3.6, 2);
                TopSpeed = Math.Round(speedValues.Max() * 3.6, 2);
                CurrentSpeed = Math.Round(CurrentSpeed * 3.6, 2);
                CurrentDistance = this.distance.LastOrDefault();
            }
        }
        /// <summary>
        /// It takes a list of doubles, and sets the AverageRate, HighestRate, and LowestRate properties to the average,
        /// max, and min of the list, respectively
        /// </summary>
        /// <param name="heartValues">A list of doubles that represent the heart rate values.</param>
        public void UpdateHeartValues(List<double> heartValues)
        {
            if (heartValues != null && heartValues.Count > 0)
            {
                AverageRate = Math.Round(heartValues.Average());
                HighestRate = Math.Round(heartValues.Max());
                LowestRate = Math.Round(heartValues.Min());
            }
            else
            {
                AverageRate = double.NaN;
                HighestRate = double.NaN;
                LowestRate = double.NaN;
            }
        }

        /// <summary>
        /// It takes a JSON object, extracts the distance array, and adds each value to the distance array
        /// </summary>
        /// <param name="JObject">This is the data that is being passed in from the JSON file.</param>
        public void AddDataDistance(JObject data)
        {
            JArray distance = (JArray)data["distance"];

            foreach (JObject key in distance)
            {
                this.distance.Add(double.Parse(key["value"]!.ToObject<string>()!, CultureInfo.InvariantCulture));
            }
        }
        /// <summary>
        /// It takes a string value, converts it to a double, divides it by 1000, rounds it to the nearest whole number, and
        /// then adds the word "Seconds" to the end of it
        /// </summary>
        /// <param name="val">The value of the time elapsed in milliseconds.</param>
        private void CalculateTimeElapsed(string val)
        {
            try
            {
                TimeElapsed = Math.Round(double.Parse(val) / 1000, 0) + " Seconds";
            }
            catch (Exception e)
            {
                TimeElapsed = "? Seconds";
            }
        }

        double speedcounter = 0;
        /// <summary>
        /// It takes a JSON object, parses it, and adds the data to the graph
        /// </summary>
        /// <param name="JObject">A JSON object.</param>
        public void AddDataSpeed(JObject data)
        {
            JArray val = (JArray)data["speed"];
            
            foreach (JObject key in val)
            {
                speedcounter++;
                double speedValue = double.Parse(key["value"]!.ToObject<string>()!, CultureInfo.InvariantCulture);
                this.speed.Add(speedValue);
                AddSpeedPoint(speedValue*3.6, speedcounter);
                CalculateTimeElapsed(key["time"].ToObject<string>()!);
            }
            CurrentSpeed = this.speed.LastOrDefault();
            // CurrentSpeed = CurrentSpeed * 3.6;
            UpdateSpeedValues(this.speed);


        }

        double heartcounter = 0;
        /// <summary>
        /// This function takes in a JSON object, parses the heart rate values from it, and adds them to the heart rate
        /// chart
        /// </summary>
        /// <param name="JObject">This is the data that is being passed in from the server.</param>
        public void AddDataHeartRate(JObject data)
        {
            JArray val = (JArray)data["heartrate"];

            foreach (JObject key in val)
            {
                heartcounter++;
                double heartRateValue = double.Parse(key["value"]!.ToObject<string>()!, CultureInfo.InvariantCulture);
                this.heartRate.Add(heartRateValue);
                AddHeartPoint((heartRateValue), heartcounter);
            }
            CurrentRate = this.heartRate.LastOrDefault();
            UpdateHeartValues(this.heartRate);
        }

        private object speedDataMapper;
        public object SpeedDataMapper
        {
            get => this.speedDataMapper;
            set
            {
                this.speedDataMapper = value;
                OnPropertyChanged(nameof(SpeedDataMapper));
            }
        }
        private object heartDataMapper;
        public object HeartDataMapper
        {
            get => this.heartDataMapper;
            set
            {
                this.heartDataMapper = value;
                OnPropertyChanged(nameof(HeartDataMapper));
            }
        }

        public ChartValues<ObservablePoint> SpeedGraphValues { get; }
        public ChartValues<ObservablePoint> HeartGraphValues { get; }
       
        //Session model constructer with no sessionname
        public SessionModel(string sessionName)
        {
            sessionint++;
            this.sessionName = sessionName + sessionint;
            distance = new List<double>();
            speed = new List<double>();
            heartRate = new List<double>();

            CurrentRate = 0;
            CurrentSpeed = 0;
            AverageRate = 0;
            AverageSpeed = 0;
            LowestRate = 0;
            CurrentDistance = 0;
            HighestRate = 0;

            this.SpeedGraphValues = new ChartValues<ObservablePoint>();
            this.HeartGraphValues = new ChartValues<ObservablePoint>();


            this.SpeedDataMapper = new CartesianMapper<ObservablePoint>().X(point => point.X).Y(point => point.Y);
            this.HeartDataMapper = new CartesianMapper<ObservablePoint>().X(point => point.X).Y(point => point.Y);

        }
        //Session model constructer with no given values

        public SessionModel()
        {
            CurrentRate = 0;
            CurrentSpeed = 0;
            AverageRate = 0;
            AverageSpeed = 0;
            LowestRate = 0;
            CurrentDistance = 0;
            HighestRate = 0;

            this.SpeedGraphValues = new ChartValues<ObservablePoint>();
            this.HeartGraphValues = new ChartValues<ObservablePoint>();


            this.SpeedDataMapper = new CartesianMapper<ObservablePoint>().X(point => point.X).Y(point => point.Y);
            this.HeartDataMapper = new CartesianMapper<ObservablePoint>().X(point => point.X).Y(point => point.Y);

        }

        public bool realTimeData = false;
        public string sessionUuid;
        
        //Session model constructer with session name and uuid
        public SessionModel(string sessionName, string uuid)
        {
            realTimeData = true;
            this.sessionUuid = uuid;
            sessionint++;
            this.sessionName = sessionName + sessionint;
            distance = new List<double>();
            speed = new List<double>();
            heartRate = new List<double>();

            this.SpeedGraphValues = new ChartValues<ObservablePoint>();
            this.HeartGraphValues = new ChartValues<ObservablePoint>();


            this.SpeedDataMapper = new CartesianMapper<ObservablePoint>().X(point => point.X).Y(point => point.Y);
            this.HeartDataMapper = new CartesianMapper<ObservablePoint>().X(point => point.X).Y(point => point.Y);

        }
     
        /// <summary>
        /// This function takes in a speed and a time and adds it to the graph
        /// </summary>
        /// <param name="speed">The speed of the car at the time of the point</param>
        /// <param name="time">The time in seconds that the speed was recorded at.</param>
        public void AddSpeedPoint(double speed, double time)
        {
            
                var point = new ObservablePoint()
                {
                    X = Math.Round(time, 2),
                    Y = Math.Round(speed,2)
                };

                this.SpeedGraphValues.Add(point);
            
        }
        /// <summary>
        /// This function takes in a heart rate and a time and adds it to the graph
        /// </summary>
        /// <param name="heartRate">The heart rate value to be added to the graph</param>
        /// <param name="time">The time in seconds that the heart rate was recorded.</param>
        public void AddHeartPoint(double heartRate, double time)
        {

            var point = new ObservablePoint()
            {
                X = Math.Round(time, 2),
                Y = Math.Round(heartRate, 2)
            };

            this.HeartGraphValues.Add(point);

        }
        /// <summary>
        /// It takes a string in the format of "yyyy-MM-dd HH:mm:ss.fff" and returns a DateTime object
        /// </summary>
        /// <param name="time">The time string to parse.</param>
        /// <returns>
        /// The method is returning a DateTime object.
        /// </returns>
        private DateTime CustomParseDate(string time)
        {
            return DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// This function initializes the variables to 0
        /// </summary>
        public void Init()
        {
            AverageSpeed = 0;
            TopSpeed = 0;
            AverageRate = 0;
            HighestRate = 0;
            LowestRate = 0;
        }

        /* A simple implementation of the INotifyPropertyChanged interface. */
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }


}
