﻿using Caliburn.Micro;
using DoctorApplication.Core;
using DoctorApplication.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApplication.MVVM.ViewModel
{

    internal class HistoryViewModel : ObservableObject
    {
        //combobox controls
        public BindableCollection<UserDataModel> users { get; set; }
        public BindableCollection<SessionModel> sessions { get; set; }


        private UserDataModel selectedUser;
        public UserDataModel SelectedUser
        {
            get { return selectedUser; }
            set{selectedUser = value; OnPropertyChanged();}
        }


        private SessionModel selectedSession;
        public SessionModel SelectedSession
        {
            get { return selectedSession; }
            set { selectedSession = value; OnPropertyChanged(); }
        }




        public HistoryViewModel(BindableCollection<UserDataModel> users)
        {
            this.users = users;
        }
    }
}