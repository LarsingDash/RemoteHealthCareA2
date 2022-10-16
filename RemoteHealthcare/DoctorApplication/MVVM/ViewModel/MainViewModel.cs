using Caliburn.Micro;
using DoctorApplication.Core;
using DoctorApplication.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public MultipleViewModel MultipleVM { get; set; }

        public BindableCollection<UserDataModel> users { get; set; }

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
            users = new BindableCollection<UserDataModel>();
            ChangeView = new RelayCommand(ChangeViewToggled);
            DataVM = new DataViewModel(users);
            HistoryVM = new HistoryViewModel(users);
            MultipleVM = new MultipleViewModel(users);
            CurrentView = DataVM;
            ToggleButtonText = "Switch to current data";
            UserDataModel test1 = new UserDataModel("user1", "0612345678", 12345);
            UserDataModel test2 = new UserDataModel("user2", "0698765432", 67890);
            UserDataModel test3 = new UserDataModel("user3", "0698665232", 98765);

            //adding testdata for view
            test1.addData(new DataModel(10, new TimeSpan(10000), 80));
            test1.addData(new DataModel(15, new TimeSpan(10000), 81));
            test1.addData(new DataModel(22, new TimeSpan(10000), 70));

            test2.addData(new DataModel(1, new TimeSpan(10000), 80));
            test2.addData(new DataModel(3, new TimeSpan(10000), 85));
            test2.addData(new DataModel(10, new TimeSpan(10000), 60));


            test3.addData(new DataModel(40, new TimeSpan(10000), 90));
            test3.addData(new DataModel(50, new TimeSpan(10000), 70));
            test3.addData(new DataModel(45, new TimeSpan(10000), 65));
            test3.addData(new DataModel(39, new TimeSpan(10000), 80));


            test1.AddSession(new SessionModel("ABS", "Session 1"));
            test1.AddSession(new SessionModel("AHS", "Session 2"));
            test1.AddSession(new SessionModel("ADS", "Session 3"));

            test2.AddSession(new SessionModel("BSS", "Session 1"));
            test2.AddSession(new SessionModel("BSD", "Session 2"));
            test2.AddSession(new SessionModel("BFS", "Session 3"));

            test3.AddSession(new SessionModel("CDS", "Session 1"));
            test3.AddSession(new SessionModel("CDF", "Session 2"));
            test3.AddSession(new SessionModel("CHS", "Session 3"));
            //adding test messages
            test1.AddMessage("Hello");
            test1.AddMessage("Im a console!");
            test1.AddMessage("Goodbye!");

            test2.AddMessage("Hi!");
            test2.AddMessage("Whats up?");

            test3.AddMessage("Go away");
            test3.AddMessage("Leave me alone");

            //assinging message lists to users
            users.Add(test1);
            users.Add(test2);
            users.Add(test3);
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
