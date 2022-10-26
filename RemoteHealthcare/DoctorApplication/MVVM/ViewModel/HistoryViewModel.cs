using Caliburn.Micro;
using ClientApplication.ServerConnection;
using ClientApplication.ServerConnection.Communication;
using DoctorApplication.Communication;
using DoctorApplication.Core;
using DoctorApplication.MVVM.Model;
using LiveCharts.Wpf;
using Newtonsoft.Json.Linq;
using Shared;
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
            BindableCollection<UserDataModel> list = new BindableCollection<UserDataModel>();
            Client client = App.GetClientInstance();
            var serial = Util.RandomString();
            client.SendEncryptedData(JsonFileReader.GetObjectAsString("AllClients", new Dictionary<string, string>()
            {
                {"_serial_", serial},
            }, JsonFolder.Json.Path));
            _ = client.AddSerialCallbackTimeout(serial, ob =>
            {
                if (ob["data"]!["status"]!.ToObject<string>()!.Equals("ok"))
                {
                    string[] userNames = ob["data"]!.Value<JArray>("users")!.Values<string>().ToArray()!;
                    foreach (var userName in userNames)
                    {  
                            list.Add(new UserDataModel(userName));
                    }
                }
                else
                {
                    // Status not ok when getting active-clients
                }
            }, () =>
            {

            }, 1000);
            this.users = list;
        }
    }
}
