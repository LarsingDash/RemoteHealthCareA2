using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClientApplication.Bike;
using ClientApplication.ServerConnection;
using ClientApplication.View;
using ClientSide.VR2;
using Shared.Log;

namespace ClientApplication
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		private static BikeHandler handler;
		private static Client client;
		private static VRClient vrClient;

		private void ApplicationStart(object sender, StartupEventArgs e)
		{
			this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
			Logger.LogMessage(LogImportance.Information, "ClientApplication Started");
			new Thread(async start =>
			{
				handler = new BikeHandler();
				BikePhysical bike = (BikePhysical) handler.Bike;
				await bike.StartConnection();
				client = new Client();
				vrClient = new VRClient();
				// vrClient.Setup();
			}).Start();
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
		
		void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
			Logger.LogMessage(LogImportance.Fatal, "Unknown", e.Exception);
			App_OnExit(null, null);
		}

		public static BikeHandler GetBikeHandlerInstance()
		{
			return handler;
		}

		public static Client GetClientConnectedToServerInstance()
		{
			return client;
		}

		public static VRClient GetVrClientInstance()
		{
			return vrClient;
		}

		private void App_OnExit(object sender, ExitEventArgs e)
		{
			Logger.LogMessage(LogImportance.DebugHighlight, "Closing Application");
			if (handler.Bike.GetType() == typeof(BikePhysical))
			{
				BikePhysical bike = (BikePhysical) handler.Bike;
				bike.Close();
			}
		}
	}
}