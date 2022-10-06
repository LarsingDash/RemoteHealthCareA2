using DoctorApplication.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoctorApplication.MVVM.Model;
using Caliburn.Micro;

namespace DoctorApplication.MVVM.ViewModel
{
    internal class DataViewModel : Screen
    {
        public BindableCollection<UserDataModel> users { get; set; }
        public ObservableCollection<MessageModel> messages { get; set; }
        
        private UserDataModel selectedUser;
        public UserDataModel SelectedUser
        {
            get { return selectedUser; }
            set
            {
                selectedUser = value;
                NotifyOfPropertyChange(() => selectedUser);


            }
        }
        public DataViewModel()
        {
            users = new BindableCollection<UserDataModel>();
            users.Add(new UserDataModel("user1", "0612345678", 12345, 20, 80));
            users.Add(new UserDataModel("user2", "0698765432", 67890, 30, 70));

        }
    }
}