using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DoctorApplication.MVVM.Model
{
    public class SessionModel : INotifyPropertyChanged
    {
        public string sessionName { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }


        TimeSpan timeElapsed;

        public TimeSpan TimeElapsed
        {
            get {
                if(endTime != null)
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

        List<double> distance = new List<double>();
        List<double> speed = new List<double>();
        List<double> heartRate = new List<double>();

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
        }
        
        public void AddDataSpeed(JObject data)
        {
            JArray val = (JArray)data["speed"];
        
            foreach (JObject key in val)
            {
                this.speed.Add(double.Parse(key["value"]!.ToObject<string>()!));
            }
        }
        
        public void AddDataHeartRate(JObject data)
        {
            JArray val = (JArray)data["heartrate"];
        
            foreach (JObject key in val)
            {
                this.heartRate.Add(double.Parse(key["value"]!.ToObject<string>()!));
            }
        }
        public SessionModel(string sessionName)
        {
            this.sessionName = sessionName;
        }
    }
}
