/* This is the model for the userdata. It contains all the data that is currently needed to be displayed. */
using Caliburn.Micro;
using DoctorApplication.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientApplication.ServerConnection;
using ClientApplication.ServerConnection.Communication;
using DoctorApplication.Communication;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Log;

namespace DoctorApplication.MVVM.Model
{
    public class UserDataModel : INotifyPropertyChanged
    {


        //userdata
        private string userName;

        //statistic data bike
        private double topSpeed;

        public double TopSpeed
        {
            get
            {
                if (userDataList != null)
                {
                    double topValue = 0;
                    foreach (DataModel dataModel in userDataList)
                    {
                        if (dataModel.CurrentSpeed > topValue)
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

        private ObservableCollection<SessionModel> sessions;

        public ObservableCollection<SessionModel> Sessions
        {
            get { return sessions; }
            set
            {
                sessions = value;
                OnPropertyChanged(nameof(sessions));
            }
        }

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
                    return (total / userDataList.Count);
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
            get { return userDataList.LastOrDefault()!; }
            set
            {
                lastEntry = value;
                OnPropertyChanged(nameof(lastEntry));
            }
        }
        public SessionModel lastSession;
        public SessionModel LastSession
        {
            get { return sessions.LastOrDefault()!; }
            set
            {
                lastSession = value;
                OnPropertyChanged(nameof(lastSession));
            }
        }
        private double lastSpeed;
        public double LastSpeed
        {
            get { return LastSession.lastSpeed; }
            set
            {
                lastSpeed = value;
                OnPropertyChanged(nameof(lastSpeed));
            }
        }
        private double lastDistance;

        public double LastDistance
        {
            get { return LastSession.lastDistance; }
            set
            {
                lastDistance = value;
                OnPropertyChanged(nameof(lastDistance));
            }
        }
        private double lastHeartRate;
        public double LastHeartRate
        {
            get { return LastSession.lastHeartRate; }
            set
            {
                lastHeartRate = value;
                OnPropertyChanged(nameof(lastHeartRate));
            }
        }




        //chatdata
        public ObservableCollection<MessageModel> messages { get; set; }

        //constructor currently with test values
        public UserDataModel()
        {
            this.UserName = "TestName";
            this.messages = new ObservableCollection<MessageModel>();
            this.userDataList = new List<DataModel>();
            this.sessions = new BindableCollection<SessionModel>();
        }

        public UserDataModel(string userName)
        {
            UserName = userName;
            this.messages = new ObservableCollection<MessageModel>();
            this.userDataList = new List<DataModel>();
            this.sessions = new BindableCollection<SessionModel>();

            Client client = App.GetClientInstance();
            var serial = Util.RandomString();
            client.SendEncryptedData(JsonFileReader.GetObjectAsString("HistoricClientData", new Dictionary<string, string>()
            {
                {"_serial_", serial},
                {"_name_", userName}
            }, JsonFolder.Json.Path));

            client.AddSerialCallbackTimeout(serial, ob =>
            {
                foreach (JObject session in ob["data"]!.Value<JArray>("bike-sessions")!.Values<JObject>())
                {
                    if (session == null) return;
                    SessionModel model = new SessionModel(session["session-name"]!.ToObject<string>()!);
                    model.AddDataDistance(session);
                    model.AddDataSpeed(session);
                    model.AddDataHeartRate(session);
                    try
                    {
                        model.startTime = CustomParseDate(session["start-time"]!.ToObject<string>()!);
                        model.endTime = !session["end-time"]!.ToObject<string>()!.Equals("_endtime_") ? CustomParseDate(session["end-time"]!.ToObject<string>()!) : DateTime.MinValue;
                    }
                    catch (Exception e)
                    {
                        Logger.LogMessage(LogImportance.Fatal, "Time could not be parsed", e);
                    }

                    sessions.Add(model);
                }
            }, () =>
            {
                // No Response from server
            }, 1000);
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
            set
            {
                userName = value;
                OnPropertyChanged(nameof(UserName));
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
        private DateTime CustomParseDate(string time)
        {
            return DateTime.ParseExact(time, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }

}
