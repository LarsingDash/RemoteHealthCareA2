using DoctorApplication.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoctorApplication.MVVM.Model;
using Caliburn.Micro;
using Newtonsoft.Json.Linq;
using System.Windows;
using LiveCharts.Wpf;
using LiveCharts;
using System.DirectoryServices;

namespace DoctorApplication.MVVM.ViewModel
{
    internal class DataViewModel : ObservableObject
    {

        private string message;

        public string Message
        {
            get { return message; }
            set { message = value;
                OnPropertyChanged();
            }
        }
      
        public RelayCommand SendCommand { get; set; }
        public RelayCommand GetUserCommand { get; set; }
        public BindableCollection<UserDataModel> users { get; set; }
        public ObservableCollection<MessageModel> messages { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private UserDataModel selectedUser;
        public UserDataModel SelectedUser
        {
            get { return selectedUser; }
            set
            {
                selectedUser = value;
                OnPropertyChanged();
            }
        }

       
        

        public DataViewModel()
        {

            //creating users (test data)
            users = new BindableCollection<UserDataModel>();
            UserDataModel test1 = new UserDataModel("user1", "0612345678", 12345);
            UserDataModel test2 = new UserDataModel("user2", "0698765432", 67890);

            //adding testdata for view
            test1.addData(new DataModel(20, 30, 25, new TimeSpan(10000), 75, 70, 75, 80));
            test2.addData(new DataModel());

            //adding test messages
            test1.AddMessage("Hello");
            test1.AddMessage("Im a console!");
            test1.AddMessage("Goodbye!");
            test2.AddMessage("Hi!");
            test2.AddMessage("Whats up?");

            //assinging message lists to users
            users.Add(test1);
            users.Add(test2);

            //initializing sendcommand 
            SendCommand = new RelayCommand(SendMessage);
            GetUserCommand = new RelayCommand(GetUser);

            //graph series initialisation
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { 4, 6, 5, 2 ,4 }
                }
            };
        }

        public void SendMessage(object Message)
        {
            selectedUser.AddMessage(Message.ToString());
        }
        public void GetUser(object user)
        {
            Console.WriteLine(user.ToString());
        }
        public void DisplayInMessageBox(object message)
        {
            Console.WriteLine(message.ToString());
        }
       
       
    }
}