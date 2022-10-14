/* This is the model for the userdata. It contains all the data that is currently needed to be displayed. */
using Caliburn.Micro;
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

        //statistic data bike
        private double topSpeed;

        public double TopSpeed
        {
            get {
                if (userDataList != null)
                {
                    double topValue = 0;
                    foreach (DataModel dataModel in userDataList)
                    {
                        if(dataModel.CurrentSpeed > topValue)
                        {
                            topValue = dataModel.CurrentSpeed;
                        }
                    }
                    return topValue;
                }
                return 0;
            }
            set
            {
                topSpeed = value;
                OnPropertyChanged(nameof(lastEntry));
            }
        }

        public ObservableCollection<SessionModel> sessions { get; set; }

        private double averageSpeed;
        public double AverageSpeed
        {
            get
            {
                if (userDataList != null)
                {
                    double total = 0;
                    foreach (DataModel dataModel in userDataList)
                    {
                        total += dataModel.CurrentSpeed;
                    }
                    return Math.Round((total / userDataList.Count), 1);
                }
                return 0;
            }
            set
            {
                averageSpeed = value;
                OnPropertyChanged(nameof(lastEntry));
            }
        }

        //statistic data heartmonitor
        private int lowestRate;

        public int LowestRate
        {
            get
            {
                if (userDataList != null)
                {
                    int lowestValue = 999;
                    foreach (DataModel dataModel in userDataList)
                    {
                        if (dataModel.CurrentRate < lowestValue)
                        {
                            lowestValue = dataModel.CurrentRate;
                        }
                    }
                    return lowestValue;
                }
                return 0;
            }
            set
            {
                lowestRate = value;
                OnPropertyChanged(nameof(lastEntry));
            }
        }


        private int averageRate;

        public int AverageRate
        {
            get
            {
                if (userDataList != null)
                {
                    int total = 0;
                    foreach (DataModel dataModel in userDataList)
                    {
                        total += dataModel.CurrentRate;
                    }
                    return (total/userDataList.Count);
                }
                return 0;
            }
            set
            {
                averageRate = value;
                OnPropertyChanged(nameof(lastEntry));
            }
        }
        private int highestRate;
        public int HighestRate
        {
            get
            {
                if (userDataList != null)
                {
                    int topValue = 0;
                    foreach (DataModel dataModel in userDataList)
                    {
                        if (dataModel.CurrentRate > topValue)
                        {
                            topValue = dataModel.CurrentRate;
                        }
                    }
                    return topValue;
                }
                return 0;
            }
            set
            {
                highestRate = value;
                OnPropertyChanged(nameof(lastEntry));
            }
        }

        public List<DataModel> userDataList { get; set; }

        public DataModel lastEntry;
        public DataModel LastEntry
        {
            get { return userDataList.LastOrDefault(); }
            set
            {
                lastEntry = value;
                OnPropertyChanged(nameof(lastEntry));
            }
        }
        //chatdata
        public ObservableCollection<MessageModel> messages { get; set; }

        //constructor currently with test values
        public UserDataModel()
        {
            this.UserName = "TestName";
            this.phoneNumber = "06 12345678";
            this.bikeId = 01249;
            this.messages = new ObservableCollection<MessageModel>();
            this.userDataList = new List<DataModel>();
            this.sessions = new BindableCollection<SessionModel>();
        }

        public UserDataModel(string userName, string phoneNumber, int bikeId)
        {
            UserName = userName;
            PhoneNumber = phoneNumber;
            BikeId = bikeId;
            this.messages = new ObservableCollection<MessageModel>();
            this.userDataList = new List<DataModel>();
            this.sessions = new BindableCollection<SessionModel>();
        }

        public void addData(DataModel dataModel)
        {
            userDataList.Add(dataModel);
        }
        public void AddSession(SessionModel sessionModel)
        {
            sessions.Add(sessionModel);
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
