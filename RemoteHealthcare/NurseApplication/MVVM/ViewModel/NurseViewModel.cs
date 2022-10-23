using NurseApplication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using NurseApplication.MVVM.Model;

namespace NurseApplication.MVVM.ViewModel
{
    internal class NurseViewModel : ObservableObject
    {
        public ObservableCollection<AlertModel> Alerts;

        public NurseViewModel()
        {
           Alerts = new ObservableCollection<AlertModel>();
        }

        public void AddAlert(string userName, string bikeId)
        {
            Alerts.Add(new AlertModel(userName, bikeId));
        }
    }
}
