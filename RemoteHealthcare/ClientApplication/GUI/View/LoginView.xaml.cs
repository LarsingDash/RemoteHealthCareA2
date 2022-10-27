using System.Windows;
using System.Windows.Input;

namespace ClientApplication.View;

public partial class LoginView : Window
{
	public LoginView()
	{
		InitializeComponent();
		txtPhoneNumber.Focus();
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

	private void TxtPhoneNumber_OnKeyDown(object sender, KeyEventArgs e)
	{
		if (System.Text.RegularExpressions.Regex.IsMatch(txtPhoneNumber.Text, "[^0-9]"))
		{
			txtPhoneNumber.Text = "";
		}
	}
}