using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
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
            Logger.PrintLevel = LogLevel.All;
            mainThread = Thread.CurrentThread;
            client = new Client();
        }

        private void ApplicationStart(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }


    }
}