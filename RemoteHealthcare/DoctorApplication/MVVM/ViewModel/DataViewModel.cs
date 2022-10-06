﻿using DoctorApplication.Core;
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




        public BindableCollection<UserDataModel> users { get; set; }
        public ObservableCollection<MessageModel> messages { get; set; }

        public RelayCommand MessageCommand { get; set; }
        public RelayCommand ConsoleLogcommand { get; set; }
        
        
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

            users = new BindableCollection<UserDataModel>();
            UserDataModel test1 = new UserDataModel("user1", "0612345678", 12345, 20, 80);
            UserDataModel test2 = new UserDataModel("user2", "0698765432", 67890, 30, 70);
            test1.AddMessage("Hello");
            test1.AddMessage("Im a console!");
            test1.AddMessage("Goodbye!");
            test2.AddMessage("Hi!");
            test2.AddMessage("Whats up?");
            users.Add(test1);
            users.Add(test2);

            MessageCommand = new RelayCommand(DisplayInMessageBox);

            SendCommand = new RelayCommand(DisplayInMessageBox);
        }
      
        public void DisplayInMessageBox(object message)
        {
            Console.WriteLine(message);
        }
       
       
    }
}