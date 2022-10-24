using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace NurseApplication.MVVM.Model
{
    internal class AlertModel
    {
        public string UserName { get; set; }
        public string BikeId { get; set; }
        public string Time { get; set; }

        public AlertModel(string userName, string bikeId)
        {
            this.UserName = userName;
            this.BikeId = bikeId;
            this.Time = DateTime.Now.ToString("HH:mm");
        }

        public override string ToString()
        {
            return "User: " + UserName + "is in danger and requires assitance! " + Time;
        }
    }
}
