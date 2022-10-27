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
using System.Globalization;
using System.Threading;
using ClientApplication.ServerConnection;
using ClientApplication.ServerConnection.Communication;
using DoctorApplication.Communication;
using Shared;
using Shared.Log;
using LiveCharts.Helpers;

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

        private bool chatTypeState;

        public bool ChatTypeState
        {
            get { return chatTypeState; }
            set
            {
                chatTypeState = value;
                OnPropertyChanged();
            }
        }
        
        private string currentSessionUuid;

        private string buttonText2;

        public string ButtonText2
        {
            get { return buttonText2; }
            set
            {
                buttonText2 = value;
                OnPropertyChanged();
            }
        }
        private string recordingText;

        public string RecordingText
        {
            get { return recordingText; }
            set
            {
                recordingText = value;
                OnPropertyChanged();
            }
        }


        //commands
        public RelayCommand SendCommand { get; set; }
        public RelayCommand GetUserCommand { get; set; }
        public RelayCommand EmergencyPressedCommand { get; set; }
        public RelayCommand StartSTopRecordingCommand { get; set; }
        public RelayCommand ChatTypeCommand { get; set; }

        //Data collections
        private BindableCollection<UserDataModel> users;
        public BindableCollection<UserDataModel> Users
        {
            get { return users; }
            set
            {
                users = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<MessageModel> Messages { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        //currently selected user in combobox

        private UserDataModel? selectedUser;
        public UserDataModel? SelectedUser
        {
            get { return selectedUser; }
            set
            {
                selectedUser = value;
                OnPropertyChanged("SelectedUser");
                LastSession.Init();
                Console.WriteLine("Selected User changed");
            }
        }

        private SessionModel lastSession;
        public SessionModel LastSession
        {
            get { return SelectedUser.LastSession; }
            set
            {
                lastSession = value;
                OnPropertyChanged();
            }
        }

        private SessionModel selectedSession;
        public SessionModel SelectedSession
        {
            get { return selectedSession; }
            set
            {
                selectedSession = value;
                OnPropertyChanged();
            }
        }

        //data for graph
        public LineSeries LineSeries;



        public DataViewModel(BindableCollection<UserDataModel> users)
        {
            //creating users (test data)
            this.Users = users;
            this.ChatTypeState = false;

            SelectedSession = new SessionModel("0000");
            //initializing sendcommand 
            SendCommand = new RelayCommand(SendMessage);
            GetUserCommand = new RelayCommand(GetUser);
            ChatTypeCommand = new RelayCommand(ChatTypeToggled);
            EmergencyPressedCommand = new RelayCommand(EmergencyFunction);
            StartSTopRecordingCommand = new RelayCommand(StartStopRecordingFunction);

            //predetermined text in button
            buttonText2 = "Single User";
            RecordingText = "Start Recording";

            dataHandler = new ConnectionHandler();
        }

        private bool isRecordingActive = false;
        private void StartStopRecordingFunction(object obj)
        {
            if (SelectedUser == null)
            {
                return;
            }
            if (!SelectedUser.isRecordingActive)
            {
                SelectedUser.isRecordingActive = true;
                SelectedUser.RecordingText = "Stop Recording";
                StartBikeRecording();
                Thread.Sleep(1000);
                OnPropertyChanged("LastSession");
                //Test to see if triggering on property changed helps.


            }
            else
            {
                SelectedUser.isRecordingActive = false;
                SelectedUser.RecordingText = "Start Recording";
                StopBikeRecording("normal");
            }
        }
        private void EmergencyFunction(object obj)
        {
            if(SelectedUser == null)
            {
                return;
            }
            StopBikeRecording("emergencyStop");
            SelectedUser.isRecordingActive =false;
            SelectedUser.RecordingText = "Start Recording";
            Console.WriteLine("Emergency Pressed!");
        }

        public async void StartBikeRecording()
        {
            Client client = App.GetClientInstance();
            var serial = Util.RandomString();
            if (selectedUser == null)
            {
                return;
            }
            client.SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecording", new Dictionary<string, string>()
            {
                {"_serial_", serial},
                {"_name_", SelectedUser.UserName},
                {"_session_", DateTime.Now.ToString(CultureInfo.InvariantCulture)}
            }, JsonFolder.Json.Path));
            await client.AddSerialCallbackTimeout(serial, ob =>
            {
                currentSessionUuid = ob["data"]!["uuid"]!.ToObject<string>()!;
                client.SendEncryptedData(JsonFileReader.GetObjectAsString("SubscribeToSession", new Dictionary<string, string>()
                {
                    {"_serial_", serial},
                    {"_uuid_", currentSessionUuid},
                }, JsonFolder.Json.Path));
                SelectedUser.AddSession(new SessionModel(DateTime.Now.ToString(CultureInfo.InvariantCulture), currentSessionUuid));
                OnPropertyChanged("LastSession");
            }, () =>
            {
          }, 1000);
            LastSession.Init();
        }
               public void StopBikeRecording(string type)
        {
            if (SelectedUser == null)
            {
                return;
            }
            Client client = App.GetClientInstance();
            var serial = Util.RandomString();
            client.SendEncryptedData(JsonFileReader.GetObjectAsString("StopBikeRecording", new Dictionary<string, string>()
            {
                {"_serial_", serial},
                {"_uuid_", currentSessionUuid},
                {"_name_", SelectedUser.UserName},
                {"_stopType_", type}
            }, JsonFolder.Json.Path));
        }

        
 

        public void SendMessage(object Message)
        {
            if (selectedUser != null && Message!="")
            {
                if (chatTypeState && Users != null) {
                    Client client = App.GetClientInstance();
                    var serial = Util.RandomString();
                    Logger.LogMessage(LogImportance.DebugHighlight, "Sending broadcast");
                    client.SendEncryptedData(JsonFileReader.GetObjectAsString("ChatMessage", new Dictionary<string, string>()
                    {
                        {"_serial_", serial},
                        {"_type_", "broadcast"},
                        {"_message_", Message.ToString()!},
                        {"_receiver_", ""}
                    }, JsonFolder.Json.Path));
                    this.Message = string.Empty;
                    client.AddSerialCallbackTimeout(serial, ob =>
                    {
                        selectedUser.AddMessage(Message.ToString());
                    }, () =>
                    {
                        //No Response from server
                    }, 1000);

                } else
                {
                    Client client = App.GetClientInstance();
                    var serial = Util.RandomString();
                    client.SendEncryptedData(JsonFileReader.GetObjectAsString("ChatMessage", new Dictionary<string, string>()
                {
                    {"_serial_", serial},
                    {"_type_", "personal"},
                    {"_message_", Message.ToString()!},
                    {"_receiver_", selectedUser.UserName}
                }, JsonFolder.Json.Path));
                    this.Message = string.Empty;
                    client.AddSerialCallbackTimeout(serial, ob =>
                    {
                        selectedUser.AddMessage(Message.ToString());
                    }, () =>
                    {
                        //No Response from server
                    }, 1000);
                }
            }
        }
        public void GetUser(object user)
        {
            Console.WriteLine(user.ToString());
        }

        public void ChatTypeToggled(object state)
        {
            if ((bool)state)
            {
                //checked
                Console.WriteLine("Type Checked");
                chatTypeState = true;
                ButtonText2 = "Broadcast";
            }
            else
            {
                //unchecked
                Console.WriteLine("Type Unchecked");
                chatTypeState = false;

                ButtonText2 = "Single User";

            }
        }

    }
}