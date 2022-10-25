﻿using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DoctorApplication.MVVM.Model
{
    public class SessionModel : INotifyPropertyChanged
    {
        public string sessionName { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }

        public int sessionint = 0;

        TimeSpan timeElapsed;

        public TimeSpan TimeElapsed
        {
            get
            {
                if (endTime != null)
                {
                    return DateTime.Now - startTime;
                }
                return endTime - startTime;
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
            get {
                return distance; }
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
            get {if (currentDistance != 0) {return currentDistance;}
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
        public void UpdateSpeedValues(List<double> speedValues)
        {
            AverageSpeed = Math.Round(speedValues.Average(), 2);
            TopSpeed = Math.Round(speedValues.Max(), 2);
        }
        public void UpdateHeartValues(List<double> heartValues)
        {
            AverageRate = Math.Round(heartValues.Average());
            HighestRate = Math.Round(heartValues.Max());
            LowestRate = Math.Round(heartValues.Min());
        }
        //statistic data heartmonitor

        private double lowestRate;
        public double LowestRate
        {
            get
            {
                    double lowest = 9999;
                    foreach (double value in HeartRate)
                    {
                        if (value < lowest)
                        {
                            lowest = value;
                        }
                    }
                    return lowest;
            }
            set
            {
                lowestRate = value;
                OnPropertyChanged(nameof(lowestRate));
            }
        }


        private double averageRate;
        public double AverageRate
        {
            get
            {
                    double total = 0;
                    foreach (double value in HeartRate)
                    {
                        total += value;
                    }
                    return Math.Round((total / HeartRate.Count), 1);
            }
            set
            {
                averageRate = value;
                OnPropertyChanged(nameof(averageRate));
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
        public double lastDistance;
        public double LastDistance
        {
            get { return distance.LastOrDefault(); }
        }
        public double lastSpeed;
        public double LastSpeed
        {

            get {
                if (lastSpeed == null)
                {
                    return 99;
                }
                return speed.LastOrDefault(); }
            set
            {
                lastSpeed = value;
                OnPropertyChanged(nameof(LastSpeed));
            }
        }
        public double lastHeartRate;
        public double LastHeartRate
        {
            get { return heartRate.LastOrDefault(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void AddDataDistance(JObject data)
        {
            JArray distance = (JArray)data["distance"];

            foreach (JObject key in distance)
            {
                this.distance.Add(double.Parse(key["value"]!.ToObject<string>()!));
            }
            CurrentDistance = this.distance.LastOrDefault();
            //TestLastSpeed = double.Parse(distance.LastOrDefault()!.ToObject<string>()!);

        }

        public void AddDataSpeed(JObject data)
        {
            JArray val = (JArray)data["speed"];

            foreach (JObject key in val)
            {
                this.speed.Add(double.Parse(key["value"]!.ToObject<string>()!));
            }
            CurrentSpeed = this.speed.LastOrDefault();
            UpdateSpeedValues(this.speed);

        }

        public void AddDataHeartRate(JObject data)
        {
            JArray val = (JArray)data["heartrate"];

            foreach (JObject key in val)
            {
                this.heartRate.Add(double.Parse(key["value"]!.ToObject<string>()!));
            }
            CurrentRate = this.heartRate.LastOrDefault();
            UpdateHeartValues(this.heartRate);
        }
        public SessionModel(string sessionName)
        {
            sessionint++;
            this.sessionName = sessionName + sessionint;
            distance = new List<double>();
            speed = new List<double>();
            heartRate = new List<double>();
        }

        public bool realTimeData = false;
        public string sessionUuid;
        public SessionModel(string sessionName, string uuid)
        {
            realTimeData = true;
            this.sessionUuid = uuid;
            sessionint++;
            this.sessionName = sessionName + sessionint;
            distance = new List<double>();
            speed = new List<double>();
            heartRate = new List<double>();
        }
    }
}
