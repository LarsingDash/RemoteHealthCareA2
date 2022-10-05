using DoctorApplication.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApplication.MVVM.Model
{
    public class UserDataModel : INotifyPropertyChanged
    {
        //userdata
        private string userName;

        public string UserName
        {
            get { return userName; }
            set { 
                userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }


        public UserDataModel()
        {
            this.UserName = "Test";
        }

       

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
