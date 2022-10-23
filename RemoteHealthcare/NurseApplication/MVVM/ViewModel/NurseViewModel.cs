﻿using NurseApplication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using NurseApplication.MVVM.Model;
using System.Media;
using Caliburn.Micro;

namespace NurseApplication.MVVM.ViewModel
{
    internal class NurseViewModel : ObservableObject
    {
        public BindableCollection<AlertModel> Alerts { get; set; }

        public NurseViewModel()
        {
           Alerts = new BindableCollection<AlertModel>();
            AddAlert("test", "123124");
            Console.WriteLine(Alerts.Count);
        }

        public void AddAlert(string userName, string bikeId)
        {
            Alerts.Add(new AlertModel(userName, bikeId));
            SystemSounds.Beep.Play();
        }
    }
}
