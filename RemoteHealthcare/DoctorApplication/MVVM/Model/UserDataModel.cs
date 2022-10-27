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
using System.Threading;
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
        public bool isRecordingActive = false;

        private string recordingText;

        public string RecordingText
        {
            get { return recordingText; }
            set
            {
                recordingText = value;
                OnPropertyChanged(nameof(RecordingText));
            }
        }
        private int sliderValue;

        public int SliderValue
        {
            get { return sliderValue; }
            set
            {
                sliderValue = value;
                OnPropertyChanged(nameof(SliderValue));
                ApplySliderValue();
            }
        }
        //userdata
        private string userName;
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

        //chatdata
        public ObservableCollection<MessageModel> messages { get; set; }

        private int currentValue = 0;
        private int waitTimer = 0;
        private bool waiting = false;
        /// <summary>
        /// It sends a message to the server with the slider value, and waits for a response
        /// </summary>
        /// <returns>
        /// The slider value is being returned.
        /// </returns>
        private void ApplySliderValue()
        {
            waitTimer = 1000;
            currentValue = sliderValue;
            if (waiting)
            {
                return;
            }
            new Thread(start =>
            {
                waiting = true;
                while (waitTimer > 0)
                {
                    Thread.Sleep(1);
                    waitTimer--;
                }
                Logger.LogMessage(LogImportance.Information, sliderValue.ToString());
                Client client = App.GetClientInstance();
                var serial = Util.RandomString();
                client.SendEncryptedData(JsonFileReader.GetObjectAsString("SetResistance", new Dictionary<string, string>()
                {
                    {"_serial_" , serial},
                    {"_resistance_" , SliderValue.ToString()},
                    {"_user_", UserName }
                }, JsonFolder.Json.Path));
                waiting = false;
            }).Start();
        }

        //constructor currently with test values
        public UserDataModel()
        {
            RecordingText = "Start";
            this.UserName = "TestName";
            this.messages = new ObservableCollection<MessageModel>();
            this.sessions = new BindableCollection<SessionModel>();
        }

        public UserDataModel(string userName)
        {
            RecordingText = "Start";
            UserName = userName;
            this.messages = new ObservableCollection<MessageModel>();
            this.sessions = new BindableCollection<SessionModel>();

            /* Getting the historic data from the server. */
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
                            model.StartTime = CustomParseDate(session["start-time"]!.ToObject<string>()!);
                            model.EndTime = !session["end-time"]!.ToObject<string>()!.Equals("_endtime_")
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
       
        
        /// <summary>
        /// This function adds a session to the list of sessions
        /// </summary>
        /// <param name="SessionModel">This is the model that contains the session information.</param>
        public void AddSession(SessionModel sessionModel)
        {
            sessions.Add(sessionModel);
        }

        /// <summary>
        /// It adds a new message to the list of messages
        /// </summary>
        /// <param name="message">The message to be added to the list of messages.</param>
        public void AddMessage(string message)
        {
            messages.Add(new MessageModel(UserName, message));
        }

        /* This is a property that is used to get and set the username. */
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
        /// <summary>
        /// It takes a string in the format of "yyyy-MM-dd HH:mm:ss.fff" and returns a DateTime object
        /// </summary>
        /// <param name="time">The time string to parse.</param>
        /// <returns>
        /// The method is returning a DateTime object.
        /// </returns>
        private DateTime CustomParseDate(string time)
        {
            return DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }
    }

}
