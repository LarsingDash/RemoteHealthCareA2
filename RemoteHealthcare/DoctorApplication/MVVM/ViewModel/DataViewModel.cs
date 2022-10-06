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

namespace DoctorApplication.MVVM.ViewModel
{
    internal class DataViewModel : Screen
    {
        private string textBoxValue;

        public string TextBoxValue
        {
            get { return textBoxValue; }
            set { textBoxValue = value; }
        }
        //commands
        public RelayCommand SendCommand { get; set; }

        public BindableCollection<UserDataModel> users { get; set; }
        public ObservableCollection<MessageModel> messages { get; set; }
        
        private UserDataModel selectedUser;
        public UserDataModel SelectedUser
        {
            get { return selectedUser; }
            set
            {
                selectedUser = value;
                NotifyOfPropertyChange(() => selectedUser);


            }
        }
        public DataViewModel()
        {
            users = new BindableCollection<UserDataModel>();
            UserDataModel test1 = new UserDataModel("user1", "0612345678", 12345, 20, 80);
            UserDataModel test2 = new UserDataModel("user2", "0698765432", 67890, 30, 70);
            test1.AddMessage("Hello");
            test1.AddMessage("How are you?");
            test1.AddMessage("Goodbye!");
            test2.AddMessage("Hi!");
            test2.AddMessage("Whats up?");
            users.Add(test1);
            users.Add(test2);

        }
        
    }
}