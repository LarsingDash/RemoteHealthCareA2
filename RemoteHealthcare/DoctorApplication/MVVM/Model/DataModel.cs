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
        private TimeSpan timeElapsed;

        //heartdata
        private int currentRate;


        private DateTime dataTime;

        public DataModel(double currentSpeed, TimeSpan timeElapsed, int currentRate)
        {
            this.currentSpeed = currentSpeed;
            this.timeElapsed = timeElapsed;
            this.currentRate = currentRate;
            this.dataTime = DateTime.Now;
        }

        public DataModel()
        {
            this.currentSpeed = 20;
            this.timeElapsed = new TimeSpan(20000);
            this.currentRate = 80;
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
