using DoctorApplication.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoctorApplication.MVVM.Model;

namespace DoctorApplication.MVVM.ViewModel
{
    internal class DataViewModel : ObservableObject
    {
        public ObservableCollection<MessageModel> messages { get; set; }
        public ObservableCollection<UserDataModel> users { get; set; }

        public DataViewModel()
        {
            messages = new ObservableCollection<MessageModel>();
            users = new ObservableCollection<UserDataModel>();

            messages.Add(new MessageModel
            {
                userName = "testUser",
                message = "testMessage",
                time = DateTime.Now,
                isNativeOrigin = false,
                firstMessage = true
            });
        }
    }
}