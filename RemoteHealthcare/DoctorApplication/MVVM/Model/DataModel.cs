using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApplication.MVVM.Model
{
    public class DataModel : INotifyPropertyChanged
    {

        //bikedata
        private double currentSpeed;
        private double topSpeed;
        private double averageSpeed;
        private TimeSpan timeElapsed;

        //heartdata
        private int currentRate;
        private int lowestRate;
        private int averageRate;
        private int highestRate;

        private DateTime dataTime;

        public DataModel(double currentSpeed, double topSpeed, double averageSpeed, TimeSpan timeElapsed, int currentRate, int lowestRate, int averageRate, int highestRate)
        {
            this.currentSpeed = currentSpeed;
            this.topSpeed = topSpeed;
            this.averageSpeed = averageSpeed;
            this.timeElapsed = timeElapsed;
            this.currentRate = currentRate;
            this.lowestRate = lowestRate;
            this.averageRate = averageRate;
            this.highestRate = highestRate;
            this.dataTime = DateTime.Now;
        }

        public DataModel()
        {
            this.currentSpeed = 20;
            this.topSpeed = 25;
            this.averageSpeed = 23;
            this.timeElapsed = new TimeSpan(20000);
            this.currentRate = 80;
            this.lowestRate = 79;
            this.averageRate = 81;
            this.highestRate = 83;
            this.dataTime = DateTime.Now;
        }

        public double CurrentSpeed
        {
            get { return currentSpeed; }
            set
            {
                currentSpeed = value;
                OnPropertyChanged(nameof(CurrentSpeed));
            }
        }
        public double TopSpeed
        {
            get { return topSpeed; }
            set
            {
                topSpeed = value;
                OnPropertyChanged(nameof(TopSpeed));
            }
        }
        public double AverageSpeed
        {
            get { return averageSpeed; }
            set
            {
                averageSpeed = value;
                OnPropertyChanged(nameof(AverageSpeed));
            }
        }
        public TimeSpan TimeElapsed
        {
            get { return timeElapsed; }
            set
            {
                timeElapsed = value;
                OnPropertyChanged(nameof(TimeElapsed));
            }
        }
        public int CurrentRate
        {
            get { return currentRate; }
            set
            {
                currentRate = value;
                OnPropertyChanged(nameof(CurrentRate));
            }
        }
        public int LowestRate
        {
            get { return lowestRate; }
            set
            {
                lowestRate = value;
                OnPropertyChanged(nameof(LowestRate));
            }
        }
        public int AverageRate
        {
            get { return averageRate; }
            set
            {
                averageRate = value;
                OnPropertyChanged(nameof(AverageRate));
            }
        }
        public int HighestRate
        {
            get { return highestRate; }
            set
            {
                highestRate = value;
                OnPropertyChanged(nameof(HighestRate));
            }
        }

        /* This is a method that is used to update the view when the model changes. */
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
