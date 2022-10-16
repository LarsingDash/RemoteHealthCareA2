using Caliburn.Micro;
using DoctorApplication.Core;
using DoctorApplication.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApplication.MVVM.ViewModel
{
    internal class MultipleViewModel : ObservableObject
    {
        public BindableCollection<UserDataModel> users { get; set; }

        public MultipleViewModel(BindableCollection<UserDataModel> users)
        {
            this.users = users;
        }
    }
}
