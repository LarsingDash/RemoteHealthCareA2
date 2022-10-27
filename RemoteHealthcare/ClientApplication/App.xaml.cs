using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
		public static Dispatcher CurrentDispatcher;

		/// <summary>
		/// The application starts, a new thread is created to start the connection to the bike and the client, and a login window
		/// is created
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="StartupEventArgs">This is the class that contains the command line arguments.</param>
		private void ApplicationStart(object sender, StartupEventArgs e)
		{
			CurrentDispatcher = this.Dispatcher;
			this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
			Logger.LogMessage(LogImportance.Information, "ClientApplication Started");
			new Thread(async start =>
			{
				handler = new BikeHandler();
				BikePhysical bike = (BikePhysical) handler.Bike;
				bike.StartConnection();
				client = new Client();
				vrClient = new VRClient();
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
		
		/// <summary>
		/// > If an unhandled exception occurs, log it and exit the application
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">The exception that was thrown.</param>
		void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
			Logger.LogMessage(LogImportance.Fatal, "Unknown", e.Exception);
			App_OnExit(null, null);
		}

		/// <summary>
		/// It returns the instance of the BikeHandler class.
		/// </summary>
		/// <returns>
		/// The instance of the BikeHandler class.
		/// </returns>
		public static BikeHandler GetBikeHandlerInstance()
		{
			return handler;
		}

		/// <summary>
		/// > This function returns the client that is connected to the server instance
		/// </summary>
		/// <returns>
		/// The client object that is connected to the server instance.
		/// </returns>
		public static Client GetClientConnectedToServerInstance()
		{
			return client;
		}

		/// <summary>
		/// > This function returns the instance of the VRClient class
		/// </summary>
		/// <returns>
		/// The vrClient variable.
		/// </returns>
		public static VRClient GetVrClientInstance()
		{
			return vrClient;
		}

		/// <summary>
		/// > When the application is closed, close the bike
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="ExitEventArgs">The event arguments for the exit event.</param>
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