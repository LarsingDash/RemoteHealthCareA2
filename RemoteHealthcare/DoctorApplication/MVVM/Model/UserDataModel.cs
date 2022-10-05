using DoctorApplication.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApplication.MVVM.Model
{
    public class UserDataModel : INotifyPropertyChanged
    {
        //userdata
        private string userName;
        private string phoneNumber;
        private int bikeId;

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

        //constructor currently with test values
        public UserDataModel()
        {
            this.UserName = "TestName";
            this.phoneNumber = "06 12345678";
            this.bikeId = 01249;

            this.currentSpeed = 1;
            this.topSpeed = 2;
            this.averageRate = 3;
            this.TimeElapsed = new TimeSpan(10000000);

            this.currentRate = 68;
            this.lowestRate = 69;
            this.averageRate = 71;
            this.highestRate = 72;

        }
        public string UserName
        {
            get { return userName; }
            set { 
                userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }
        public int BikeId
        {
            get { return bikeId; }
            set
            {
                bikeId = value;
                OnPropertyChanged(nameof(BikeId));
            }
        }
        public string PhoneNumber
        {
            get { return phoneNumber; }
            set
            {
                phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
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


       

       

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
