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

            //UserDataModel test1 = new UserDataModel("user1");
            //SessionModel testSession = new SessionModel("testSession");
            //testSession.Distance.InsertRange(testSession.Distance.Count, new double[] {1,2,3,4,5,6,7,8,9,10 });
            //testSession.Speed.InsertRange(testSession.Distance.Count, new double[] {1,2,3,4,5,6,7,8, 9,10 });
            //testSession.HeartRate.InsertRange(testSession.Distance.Count, new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

            //test1.AddSession(testSession);
            //users.Add(test1);
            // UserDataModel test2 = new UserDataModel("user2", "0698765432", 67890);
            // UserDataModel test3 = new UserDataModel("user3", "0698665232", 98765);
            //
            // //adding testdata for view
            // test1.addData(new DataModel(10, new TimeSpan(10000), 80));
            // test1.addData(new DataModel(15, new TimeSpan(10000), 81));
            // test1.addData(new DataModel(22, new TimeSpan(10000), 70));
            //
            // test2.addData(new DataModel(1, new TimeSpan(10000), 80));
            // test2.addData(new DataModel(3, new TimeSpan(10000), 85));
            // test2.addData(new DataModel(10, new TimeSpan(10000), 60));
            //
            //
            // test3.addData(new DataModel(40, new TimeSpan(10000), 90));
            // test3.addData(new DataModel(50, new TimeSpan(10000), 70));
            // test3.addData(new DataModel(45, new TimeSpan(10000), 65));
            // test3.addData(new DataModel(39, new TimeSpan(10000), 80));
            //
            //
            // test1.AddSession(new SessionModel("ABS", "Session 1"));
            // test1.AddSession(new SessionModel("AHS", "Session 2"));
            // test1.AddSession(new SessionModel("ADS", "Session 3"));
            //
            // test2.AddSession(new SessionModel("BSS", "Session 1"));
            // test2.AddSession(new SessionModel("BSD", "Session 2"));
            // test2.AddSession(new SessionModel("BFS", "Session 3"));
            //
            // test3.AddSession(new SessionModel("CDS", "Session 1"));
            // test3.AddSession(new SessionModel("CDF", "Session 2"));
            // test3.AddSession(new SessionModel("CHS", "Session 3"));
            // //adding test messages
            // test1.AddMessage("Hello");
            // test1.AddMessage("Im a console!");
            // test1.AddMessage("Goodbye!");
            //
            // test2.AddMessage("Hi!");
            // test2.AddMessage("Whats up?");
            //
            // test3.AddMessage("Go away");
            // test3.AddMessage("Leave me alone");
            //
            // //assinging message lists to users
            // users.Add(test1);
            // users.Add(test2);
            // users.Add(test3);

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
