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
    internal class SessionModel : INotifyPropertyChanged
    {
        string uuid { get; set; }
        string sessionName { get; set; }
        DateTime startTime { get; set; }
        DateTime endTime { get; set; }


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
        List<int> heartRate = new List<int>();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void AddData(JObject data)
        {
                JArray distance = (JArray)data["data"]["distance"];
            
                foreach (JObject key in distance)
                {
                    this.distance.Add(double.Parse(key["value"]!.ToObject<string>()!));
                }

            

        }
        public SessionModel(string uuid, string sessionName)
        {
            this.uuid = uuid;
            this.sessionName = sessionName;
        }
    }
}
