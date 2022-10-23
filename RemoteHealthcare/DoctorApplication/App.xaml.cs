using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DoctorApplication.View;
using ClientApplication.ServerConnection.Communication;
using DoctorApplication.Communication;
using Shared.Log;

namespace ClientApplication.ServerConnection
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Client client;
        private static Thread mainThread;
        public static Client GetClientInstance()
        {
            return client;
        }
        public static Thread GetThreadInstance()
        {
            return mainThread;
        }
        public App()
        {
            mainThread = Thread.CurrentThread;
            Logger.PrintLevel = LogLevel.All;
             client = new Client();
        }

        private void ApplicationStart(object sender, StartupEventArgs e)
        {
            var loginView = new LoginView();
            loginView.Show();
            loginView.IsVisibleChanged += (s, ev) =>
            {
                if (loginView.IsVisible == false && loginView.IsLoaded)
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    loginView.Close();
                }
            };
        }

       
    }
}