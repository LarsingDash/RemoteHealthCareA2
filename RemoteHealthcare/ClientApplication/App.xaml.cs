using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ClientApplication.ServerConnection.Bike;
using ClientApplication.View;
using Shared.Log;

namespace ClientApplication
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static BikeHandler handler;

		private void ApplicationStart(object sender, StartupEventArgs e)
		{
			Logger.LogMessage(LogImportance.Information, "ClientApplication Started.");
			handler = new BikeHandler();
			var loginView = new LoginView();
			loginView.Show();
			loginView.IsVisibleChanged += (s, ev) =>
			{
				if (loginView.IsVisible==false && loginView.IsLoaded)
				{
					var mainWindow = new MainWindow();
					mainWindow.Show();
					loginView.Close();
				}
			};
		}

		public static BikeHandler GetBikeHandlerInstance()
		{
			return handler;
		}
	}
}