using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DoctorApplication.Core
{
    /* > The ObservableObject class implements the INotifyPropertyChanged interface, which means that it can be used to
    notify the UI when a property changes */
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// If the PropertyChanged event is not null, invoke it with the current object and a new PropertyChangedEventArgs
        /// object with the name of the property that changed
        /// </summary>
        /// <param name="name">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
