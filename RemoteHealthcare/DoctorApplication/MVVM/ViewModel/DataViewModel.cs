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
        //WPF Text change strings
        private string message;

        public string Message
        {
            get { return message; }
            set
            {
                message = value;
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
        }

        private bool isRecordingActive = false;

        /// <summary>
        /// The function is called when the user clicks the "Start Recording" button. The function checks to see if the user
        /// is already recording. If the user is not recording, the function sets the user's recording status to true,
        /// changes the button text to "Stop Recording", and calls the StartBikeRecording() function. If the user is already
        /// recording, the function sets the user's recording status to false, changes the button text to "Start Recording",
        /// and calls the StopBikeRecording() function
        /// </summary>
        /// <param name="obj">This is the object that is passed to the function. In this case, it's the button that was
        /// clicked.</param>
        /// <returns>
        /// the last session of the user.
        /// </returns>
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
                if(HistoryViewModel.Model != null && SelectedSession != null)
                    foreach (var user in HistoryViewModel.Model.Users)
                    {
                        if (user.UserName == SelectedUser.UserName)
                        {
                            Logger.LogMessage(LogImportance.DebugHighlight, "Adding session");
                            user.AddSession(SelectedUser.LastSession);
                            // if(HistoryViewModel.Model.SelectedUser != null)
                            // if (HistoryViewModel.Model.SelectedUser.UserName == SelectedUser.UserName)
                            // {
                            //     HistoryViewModel.Model.sessions.Add(SelectedUser.LastSession);
                            // }
                        }
                    }
            }
        }

        /// <summary>
        /// The function is called when the emergency button is pressed. It stops the recording and sets the recording text
        /// to "Start Recording"
        /// </summary>
        /// <param name="obj">This is the object that is passed to the function. In this case, it is the button that is
        /// pressed.</param>
        /// <returns>
        /// The method is returning the value of the selected user's isRecordingActive property.
        /// </returns>
        private void EmergencyFunction(object obj)
        {
            if (SelectedUser == null)
            {
                return;
            }

            StopBikeRecording("emergencyStop");
            SelectedUser.isRecordingActive = false;
            SelectedUser.RecordingText = "Start Recording";
            Console.WriteLine("Emergency Pressed!");
        }

        /// <summary>
        /// It starts a new bike recording session for the selected user
        /// </summary>
        /// <returns>
        /// The last session of the selected user.
        /// </returns>
        public async void StartBikeRecording()
        {
            Client client = App.GetClientInstance();
            var serial = Util.RandomString();
            if (selectedUser == null)
            {
                return;
            }

            client.SendEncryptedData(JsonFileReader.GetObjectAsString("StartBikeRecording",
                new Dictionary<string, string>()
                {
                    { "_serial_", serial },
                    { "_name_", SelectedUser.UserName },
                    { "_session_", DateTime.Now.ToString(CultureInfo.InvariantCulture) }
                }, JsonFolder.Json.Path));
            await client.AddSerialCallbackTimeout(serial, ob =>
            {
                currentSessionUuid = ob["data"]!["uuid"]!.ToObject<string>()!;
                client.SendEncryptedData(JsonFileReader.GetObjectAsString("SubscribeToSession",
                    new Dictionary<string, string>()
                    {
                        { "_serial_", serial },
                        { "_uuid_", currentSessionUuid },
                    }, JsonFolder.Json.Path));
                SelectedUser.AddSession(new SessionModel(DateTime.Now.ToString(CultureInfo.InvariantCulture),
                    currentSessionUuid));
                OnPropertyChanged("LastSession");
            }, () => { }, 1000);
            LastSession.Init();
        }

        /// <summary>
        /// > This function sends a JSON object to the server, which then stops the recording of the bike
        /// </summary>
        /// <param name="type">The type of stop. This can be either "manual" or "auto".</param>
        /// <returns>
        /// A JSON object with the following fields:
        /// </returns>
        public void StopBikeRecording(string type)
        {
            if (SelectedUser == null)
            {
                return;
            }

            Client client = App.GetClientInstance();
            var serial = Util.RandomString();
            client.SendEncryptedData(JsonFileReader.GetObjectAsString("StopBikeRecording",
                new Dictionary<string, string>()
                {
                    { "_serial_", serial },
                    { "_uuid_", currentSessionUuid },
                    { "_name_", SelectedUser.UserName },
                    { "_stopType_", type }
                }, JsonFolder.Json.Path));
        }


        /// <summary>
        /// It sends a message to the server, which then sends it to the selected user
        /// </summary>
        /// <param name="Message">The message to send</param>
        public void SendMessage(object Message)
        {
            if (selectedUser != null && Message != "")
            {
                if (chatTypeState && Users != null)
                {
                    Client client = App.GetClientInstance();
                    var serial = Util.RandomString();
                    Logger.LogMessage(LogImportance.DebugHighlight, "Sending broadcast");
                    client.SendEncryptedData(JsonFileReader.GetObjectAsString("ChatMessage",
                        new Dictionary<string, string>()
                        {
                            { "_serial_", serial },
                            { "_type_", "broadcast" },
                            { "_message_", Message.ToString()! },
                            { "_receiver_", "" }
                        }, JsonFolder.Json.Path));
                    this.Message = string.Empty;
                    client.AddSerialCallbackTimeout(serial, ob => { selectedUser.AddMessage(Message.ToString()); },
                        () =>
                        {
                            //No Response from server
                        }, 1000);
                }
                else
                {
                    Client client = App.GetClientInstance();
                    var serial = Util.RandomString();
                    client.SendEncryptedData(JsonFileReader.GetObjectAsString("ChatMessage",
                        new Dictionary<string, string>()
                        {
                            { "_serial_", serial },
                            { "_type_", "personal" },
                            { "_message_", Message.ToString()! },
                            { "_receiver_", selectedUser.UserName }
                        }, JsonFolder.Json.Path));
                    this.Message = string.Empty;
                    client.AddSerialCallbackTimeout(serial, ob => { selectedUser.AddMessage(Message.ToString()); },
                        () =>
                        {
                            //No Response from server
                        }, 1000);
                }
            }
        }

        /// <summary>
        /// This function takes an object as a parameter and prints it to the console
        /// </summary>
        /// <param name="user">The user object that is being passed in.</param>
        public void GetUser(object user)
        {
            Console.WriteLine(user.ToString());
        }

        /// <summary>
        /// If the user has checked the chat type toggle, then the chat type is set to broadcast, otherwise it is set to
        /// single user
        /// </summary>
        /// <param name="state">The state of the toggle button.</param>
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