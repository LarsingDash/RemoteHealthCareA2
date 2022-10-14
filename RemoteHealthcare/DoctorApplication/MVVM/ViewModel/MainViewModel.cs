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
        public HistoryViewModel HistoryVM { get; }
        public DataViewModel DataVM { get; set; }

        private string toggleButtonText;

        public string ToggleButtonText
        {
            get { return toggleButtonText; }
            set
            {
                toggleButtonText = value;
                OnPropertyChanged();
            }
        }
        public RelayCommand ChangeView { get; set; }


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
            ChangeView = new RelayCommand(ChangeViewToggled);
            DataVM = new DataViewModel();
            HistoryVM = new HistoryViewModel();
            CurrentView = DataVM;
            ToggleButtonText = "Switch to current data";
        }

        public void ChangeViewToggled(object state)
        {
            if ((bool)state)
            {
                //checked
                Console.WriteLine("Switch to history");
                ToggleButtonText = "History View";
                CurrentView = HistoryVM;
            }
            else
            {
                //unchecked
                Console.WriteLine("Data View Unchecked");
                ToggleButtonText = "Switch to current data";
                CurrentView = DataVM;

            }
        }
    }
}
