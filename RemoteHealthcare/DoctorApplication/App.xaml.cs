using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DoctorApplication.Communication;

namespace DoctorApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Client client;
        public static Client GetClientInstance()
        {
            return client;
        }
        public App()
        {
             client = new Client();
        }
    }
}