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
	
	/// <summary>
	/// If the left mouse button is pressed, then move the window
	/// </summary>
	/// <param name="sender">The object that raised the event.</param>
	/// <param name="MouseButtonEventArgs">This is the event that is triggered when the mouse button is pressed.</param>
	private void LoginView_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			DragMove();
		}
	}

	/// <summary>
	/// When the minimize button is clicked, the window state is set to minimized
	/// </summary>
	/// <param name="sender">The object that raised the event.</param>
	/// <param name="RoutedEventArgs">This is the event that is being handled.</param>
	private void BtnMinimize_OnClick(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	/// <summary>
	/// When the user clicks the close button, the application will close
	/// </summary>
	/// <param name="sender">The object that raised the event.</param>
	/// <param name="RoutedEventArgs">This is the event arguments that are passed to the event handler.</param>
	private void BtnClose_OnClick(object sender, RoutedEventArgs e)
	{
		Application.Current.Shutdown();
	}

	/// <summary>
	/// If the text in the textbox is not a number, then clear the textbox
	/// </summary>
	/// <param name="sender">The object that raised the event.</param>
	/// <param name="KeyEventArgs">This is the event that is triggered when a key is pressed.</param>
	private void TxtPhoneNumber_OnKeyDown(object sender, KeyEventArgs e)
	{
		if (System.Text.RegularExpressions.Regex.IsMatch(txtPhoneNumber.Text, "[^0-9]"))
		{
			txtPhoneNumber.Text = "";
		}
	}
}