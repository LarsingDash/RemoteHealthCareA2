using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NurseApplication.Communication;
using Shared.Log;

namespace NurseApplication
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
            Logger.PrintLevel = LogLevel.All;
            client = new Client();
        }

        private void ApplicationStart(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }


    }
}