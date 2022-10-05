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
        private object _currentView;
        public DataViewModel DataVM { get; set; }

        public object CurrentView
        {
            get { return _currentView; }
            set {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            DataVM = new DataViewModel();
            CurrentView = DataVM;
        }
    }
}
