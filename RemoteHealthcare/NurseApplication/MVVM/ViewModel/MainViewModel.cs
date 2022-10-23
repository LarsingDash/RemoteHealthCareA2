using NurseApplication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NurseApplication.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        private object currentView;

        public object CurrentView
        {
            get { return currentView; }
            set {
                currentView = value;
                OnPropertyChanged();
            }
        }

        public NurseViewModel NurseVM { get; set; }

        public MainViewModel()
        {
            NurseVM = new NurseViewModel();
            CurrentView = NurseVM;
        }
    }
}
