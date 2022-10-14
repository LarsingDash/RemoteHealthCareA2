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
using LiveCharts.Wpf;
using LiveCharts;
using System.DirectoryServices;
using ClientApplication.ServerConnection;
using ClientApplication.ServerConnection.Communication;
using Shared;
using Shared.Log;

namespace DoctorApplication.MVVM.ViewModel
{
    internal class DataViewModel : ObservableObject
    {
        private ConnectionHandler dataHandler;
        
        //WPF Text change strings
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value;
                OnPropertyChanged();
            }
        }

        private string buttonText;

        public string ButtonText
        {
            get { return buttonText; }
            set
            {
                buttonText = value;
                OnPropertyChanged();
            }
        }

        //data for graph
        public LineSeries lineSeries = new LineSeries
        {
            Title = "Series 1",
            Values = new ChartValues<double> { 4, 6, 5, 2, 4 }
        };
        //commands
        public RelayCommand SendCommand { get; set; }
        public RelayCommand GetUserCommand { get; set; }
        public RelayCommand StartRecordingCommand { get; set; }

        //Data collections
        public BindableCollection<UserDataModel> users { get; set; }
        public ObservableCollection<MessageModel> messages { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        //currently selected user in combobox

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

        
       
        

        public DataViewModel(BindableCollection<UserDataModel> users)
        {

            //creating users (test data)
            this.users = users;
           

            //initializing sendcommand 
            SendCommand = new RelayCommand(SendMessage);
            GetUserCommand = new RelayCommand(GetUser);
            StartRecordingCommand = new RelayCommand(StartRecordingToggled);

            //predetermined text in button
            buttonText = "Start";

            //graph series initialisation
            SeriesCollection = new SeriesCollection
            {
                lineSeries
            };

            dataHandler = new ConnectionHandler();
            Task task = dataHandler.StartRecordingAsync("Testing");
            if (task.IsCompleted)
            {
                Task task1 = dataHandler.SubscribeToSessionAsync();
            }
            dataHandler.ListClients();
        }

        public void StartBikeRecording()
        {
            //todo start session


        }
        public void StopBikeRecording()
        {
            //todo stop session
        }

        public void SendMessage(object Message)
        {
            selectedUser.AddMessage(Message.ToString());
            this.Message = string.Empty;
            users.Add(new UserDataModel());
        }
        public void GetUser(object user)
        {
            Console.WriteLine(user.ToString());
        }
        public void StartRecordingToggled(object state)
        {
            if ((bool)state)
            {
                //checked
                Console.WriteLine("Checked");
                ButtonText = "Stop";
                StartBikeRecording();
            }
            else
            {
                //unchecked
                Console.WriteLine("Unchecked");
                ButtonText = "Start";
                StopBikeRecording();

            }
        }
       
    }
}