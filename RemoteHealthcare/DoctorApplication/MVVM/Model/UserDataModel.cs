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
using System.Windows.Controls.Primitives;
using Newtonsoft.Json;

namespace DoctorApplication.MVVM.Model
{
    public class UserDataModel : INotifyPropertyChanged
    {


        //userdata
        private string userName;

        //statistic data bike
       

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

        private double distance;
        public double Distance
        {
            get { return distance; }
            set { distance = value; OnPropertyChanged(nameof(distance)); }
        }


        private double topSpeed;
        public double TopSpeed
        {
            get
            {
                if (lastSession != null)
                {
                    double highest = 0;
                    foreach (double value in LastSession.Speed)
                    {
                        if (value > highest)
                        {
                            highest = value;
                        }
                    }
                    return highest;
                }
                return 0;
            }
            set
            {
                topSpeed = value;
                OnPropertyChanged(nameof(topSpeed));
            }
        }

        private double averageSpeed;
        public double AverageSpeed
        {
            get{
                if (lastSession != null)
                {
                    double total = 0;
                    foreach (double value in LastSession.Speed)
                    {
                        total += value;
                    }
                    return Math.Round((total / LastSession.Speed.Count), 1);
                }
                return 0;
            }
            set{
                averageSpeed = value;
                OnPropertyChanged(nameof(averageSpeed));
            }
        }
        //statistic data heartmonitor

        private double lowestRate;
        public double LowestRate
        {
            get
            {
                if (lastSession != null)
                {
                    double lowest = 9999;
                    foreach (double value in LastSession.HeartRate)
                    {
                        if (value < lowest)
                        {
                            lowest = value;
                        }
                    }
                    return lowest;
                }
                return 0;
            }
            set
            {
                lowestRate = value;
                OnPropertyChanged(nameof(lowestRate));
            }
        }
 

        private double averageRate;
        public double AverageRate
        {
            get
            {
                if (lastSession != null)
                {
                    double total = 0;
                    foreach (double value in LastSession.HeartRate)
                    {
                        total += value;
                    }
                    return Math.Round((total / LastSession.HeartRate.Count), 1);
                }
                return 0;
            }
            set
            {
                averageRate = value;
                OnPropertyChanged(nameof(averageRate));
            }
        }
        private double highestRate;
        public double HighestRate
        {
            get
            {
                if (lastSession != null)
                {
                    double highest = 0;
                    foreach (double value in LastSession.HeartRate)
                    {
                        if (value > highest)
                        {
                            highest = value;
                        }
                    }
                    return highest;
                }
                return 0;
            }
            set
            {
                highestRate = value;
                OnPropertyChanged(nameof(highestRate));
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
            this.sessions = new BindableCollection<SessionModel>();
        }

        public UserDataModel(string userName)
        {
            UserName = userName;
            this.messages = new ObservableCollection<MessageModel>();
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
                foreach (string name in ob["data"]!.Value<JArray>("bike-sessions")!.Values<string>())
                {
                    if(name == null)
                        continue;
                    serial = Util.RandomString();
                    client.SendEncryptedData(JsonFileReader.GetObjectAsString("HistoricClientDataSession", new Dictionary<string, string>()
                    {
                        {"_serial_", serial},
                        {"_username_", userName},
                        {"_session_", name}
                    }, JsonFolder.Json.Path));

                    client.AddSerialCallback(serial, sessionOb =>
                    {
                        if (!sessionOb["data"]!["status"]!.ToObject<string>()!.Equals("ok"))
                        {
                            return;
                        }
                        JObject session = sessionOb["data"]!["session"]!.ToObject<JObject>()!;
                        SessionModel model = new SessionModel(session["session-name"]!.ToObject<string>()!);
                        model.AddDataDistance(session);
                        model.AddDataSpeed(session);
                        model.AddDataHeartRate(session);
                        try
                        {
                            model.startTime = CustomParseDate(session["start-time"]!.ToObject<string>()!);
                            model.endTime = !session["end-time"]!.ToObject<string>()!.Equals("_endtime_")
                                ? CustomParseDate(session["end-time"]!.ToObject<string>()!)
                                : DateTime.MinValue;
                        }
                        catch (Exception e)
                        {
                            Logger.LogMessage(LogImportance.Fatal, "Time could not be parsed", e);
                        }

                        sessions.Add(model);
                    });
                }
            }, () =>
            {
                // No Response from server
            }, 1000);
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
            return DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }
    }

}
