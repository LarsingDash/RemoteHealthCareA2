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

		private void PnlControlBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

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

		private void MinBtn_OnClick(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}
	}
}