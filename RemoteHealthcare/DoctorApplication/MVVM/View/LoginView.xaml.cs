using System.Windows;
using System.Windows.Input;

namespace DoctorApplication.View;

public partial class LoginView : Window
{
	public LoginView()
	{
		InitializeComponent();
	}

	private void LoginView_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			DragMove();
		}
	}

	private void BtnMinimize_OnClick(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	private void BtnClose_OnClick(object sender, RoutedEventArgs e)
	{
		Application.Current.Shutdown();
	}
}