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

        private int sliderValue;

        public int SliderValue
        {
            get { return sliderValue; }
            set
            {
                sliderValue = value;
                OnPropertyChanged();
                ApplySliderValue();
            }
        }

        
        private string currentSessionUuid;

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


        //commands
        public RelayCommand SendCommand { get; set; }
        public RelayCommand GetUserCommand { get; set; }
        public RelayCommand StartRecordingCommand { get; set; }
        public RelayCommand EmergencyPressedCommand { get; set; }
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
        public ObservableCollection<MessageModel> messages { get; set; }
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
        public LineSeries lineSeries;



        public DataViewModel(BindableCollection<UserDataModel> users)
        {
            //creating users (test data)
            this.Users = users;
            this.ChatTypeState = false;

            //initializing sendcommand 
            SendCommand = new RelayCommand(SendMessage);
            GetUserCommand = new RelayCommand(GetUser);
            StartRecordingCommand = new RelayCommand(StartRecordingToggled);
            ChatTypeCommand = new RelayCommand(ChatTypeToggled);
            EmergencyPressedCommand = new RelayCommand(EmergencyFunction);

            //predetermined text in button
            buttonText = "Start";
            buttonText2 = "Single User";

            dataHandler = new ConnectionHandler();
        }

        private void EmergencyFunction(object obj)
        {
            StopBikeRecording("emergencyStop");
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
                {"_name_", selectedUser.UserName},
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
                selectedUser.AddSession(new SessionModel(DateTime.Now.ToString(CultureInfo.InvariantCulture), currentSessionUuid));
            }, () =>
            {
          }, 1000);
        }
        private void ApplySliderValue()
        {
            if (selectedUser == null)
            {
                return;
            }
            Logger.LogMessage(LogImportance.Information, sliderValue.ToString());
            Client client = App.GetClientInstance();
            var serial = Util.RandomString();
            client.SendEncryptedData(JsonFileReader.GetObjectAsString("SetResistance", new Dictionary<string, string>()
            {
                {"_serial_" , serial},
                {"_resistance_" , SliderValue.ToString()},
                {"_user_", selectedUser.UserName }
            }, JsonFolder.Json.Path));
        }
        public void StopBikeRecording(string type)
        {
            if (selectedUser == null)
            {
                return;
            }
            Client client = App.GetClientInstance();
            var serial = Util.RandomString();
            client.SendEncryptedData(JsonFileReader.GetObjectAsString("StopBikeRecording", new Dictionary<string, string>()
            {
                {"_serial_", serial},
                {"_uuid_", currentSessionUuid},
                {"_name_", selectedUser.UserName},
                {"_stopType_", type}
            }, JsonFolder.Json.Path));
        }

        public void SendMessage(object Message)
        {
            if (selectedUser != null && Message!="")
            {
                if (chatTypeState && Users != null) {
                    foreach (UserDataModel user in Users)
                    {
                Client client = App.GetClientInstance();
                var serial = Util.RandomString();
                client.SendEncryptedData(JsonFileReader.GetObjectAsString("ChatMessage", new Dictionary<string, string>()
                {
                    {"_serial_", serial},
                    {"_type_", "personal"},
                    {"_message_", Message.ToString()!},
                    {"_receiver_", user.UserName}
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
                StopBikeRecording("normal");

            }
        }
        public void ChatTypeToggled(object state)
        {
            if ((bool)state)
            {
                //checked
                Console.WriteLine("Type Checked");
                ButtonText2 = "Broadcast";
            }
            else
            {
                //unchecked
                Console.WriteLine("Type Unchecked");
                ButtonText2 = "Single User";

            }
        }

    }
}