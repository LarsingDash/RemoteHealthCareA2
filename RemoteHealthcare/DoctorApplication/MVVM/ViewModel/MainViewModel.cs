using Caliburn.Micro;
using DoctorApplication.Core;
using DoctorApplication.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientApplication.ServerConnection;
using ClientApplication.ServerConnection.Communication;
using DoctorApplication.Communication;
using Newtonsoft.Json.Linq;
using Shared;

namespace DoctorApplication.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        private object currentView;
        public HistoryViewModel HistoryVM { get; }
        public DataViewModel DataVM { get; set; }
        public MultipleViewModel MultipleVM { get; set; }

        public BindableCollection<UserDataModel> users { get; set; }

        public RelayCommand ChangeViewHomeCommand { get; set; }
        public RelayCommand ChangeViewMultiCommand { get; set; }
        public RelayCommand ChangeViewHistoryCommand { get; set; }


        /* A property that is used to set the current view. */
        public object CurrentView
        {
            get { return currentView; }
            set {
                currentView = value;
                OnPropertyChanged();
            }
        }
        
        

        /* Creating a new instance of the DataViewModel and setting the CurrentView to that instance. */
        public MainViewModel()
        {
            users = new BindableCollection<UserDataModel>();
            ChangeViewHomeCommand = new RelayCommand(SetViewToHome);
            ChangeViewMultiCommand = new RelayCommand(setViewToMulti);
            ChangeViewHistoryCommand = new RelayCommand(SetViewToHistory);
            DataVM = new DataViewModel(users);
            HistoryVM = new HistoryViewModel(users);
            MultipleVM = new MultipleViewModel(users);
            CurrentView = DataVM;


            Client client = App.GetClientInstance();
            var serial = Util.RandomString();
            client.SendEncryptedData(JsonFileReader.GetObjectAsString("ActiveClients", new Dictionary<string, string>()
            {
                {"_serial_", serial},
            }, JsonFolder.Json.Path));
            client.AddSerialCallbackTimeout(serial, ob =>
            {
                if (ob["data"]!["status"]!.ToObject<string>()!.Equals("ok"))
                {
                    string[] userNames = ob["data"]!.Value<JArray>("users")!.Values<string>().ToArray()!;
                    foreach (var userName in userNames)
                    {
                        users.Add(new UserDataModel(userName));
                    }
                }
                else
                {
                    // Status not ok when getting active-clients
                }
            }, () =>
            {
    
            }, 1000);
        }

        public void SetViewToHome(object state)
        {
            CurrentView = DataVM;

        }
        public void setViewToMulti(object state)
        {
            CurrentView = MultipleVM;

        }
        public void SetViewToHistory(object state)
        {
            CurrentView = HistoryVM;

        }
       
    }
}
