/* This is the model for the userdata. It contains all the data that is currently needed to be displayed. */
using DoctorApplication.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApplication.MVVM.Model
{
    public class UserDataModel : INotifyPropertyChanged
    {
       

        //userdata
        private string userName;
        private string phoneNumber;
        private int bikeId;

        public List<DataModel> userDataList { get; set; }

        //chatdata
        public ObservableCollection<MessageModel> messages { get; set; }

        //constructor currently with test values
        public UserDataModel()
        {
            this.UserName = "TestName";
            this.phoneNumber = "06 12345678";
            this.bikeId = 01249;

        }

        public UserDataModel(string userName, string phoneNumber, int bikeId)
        {
            UserName = userName;
            PhoneNumber = phoneNumber;
            BikeId = bikeId;
            this.messages = new ObservableCollection<MessageModel>();
            this.userDataList = new List<DataModel>();
        }

        public void addData(DataModel dataModel)
        {
            userDataList.Add(dataModel);
        }
        
        public void AddMessage(string message)
        {
            messages.Add(new MessageModel(UserName, message));
        }

        public string UserName
        {
            get { return userName; }
            set { 
                userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }
        public int BikeId
        {
            get { return bikeId; }
            set
            {
                bikeId = value;
                OnPropertyChanged(nameof(BikeId));
            }
        }
        public string PhoneNumber
        {
            get { return phoneNumber; }
            set
            {
                phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }







        /* This is a method that is used to update the view when the model changes. */
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
