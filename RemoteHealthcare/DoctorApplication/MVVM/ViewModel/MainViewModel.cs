using DoctorApplication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApplication.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        private object currentView;
        public DataViewModel DataVM { get; set; }

        /* A property that is used to set the current view. */
        public object CurrentView
        {
            get { return currentView; }
            set {
                currentView = value;
                OnPropertyChanged();
            }
        }

        /* Creating a new instance of the DataViewModel and setting the CurrentView to that instance. */
        public MainViewModel()
        {
            DataVM = new DataViewModel();
            CurrentView = DataVM;
        }
    }
}
