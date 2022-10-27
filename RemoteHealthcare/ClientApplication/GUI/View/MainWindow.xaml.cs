using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientApplication
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
			
		}

		/// <summary>
		/// When the user clicks on the control bar, the window is moved
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">This is the event that is triggered when the mouse button is pressed.</param>
		private void PnlControlBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}

		/// <summary>
		/// When the user clicks the close button, the application will close
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">This is the event arguments that are passed to the event handler.</param>
		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		/// <summary>
		/// If the window is maximized, set it to normal, otherwise set it to maximized
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">This is the event that is being handled.</param>
		private void MaxBtn_OnClick(object sender, RoutedEventArgs e)
		{
			if (WindowState == WindowState.Maximized)
			{
				WindowState = WindowState.Normal;
			}
			else
			{
				WindowState = WindowState.Maximized;
			}
		}

		/// <summary>
		/// When the Minimize button is clicked, the window state is set to minimized
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">This is the event that is being handled.</param>
		private void MinBtn_OnClick(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}
	}
}