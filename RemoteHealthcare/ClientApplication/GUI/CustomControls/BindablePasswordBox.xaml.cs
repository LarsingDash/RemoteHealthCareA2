using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace ClientApplication.CustomControls;

public partial class BindablePasswordBox : UserControl
{

	public static readonly DependencyProperty PasswordProperty =
		DependencyProperty.Register("Password", typeof(SecureString), typeof(BindablePasswordBox));
	
	public SecureString Password
	{
		get { return (SecureString)GetValue(PasswordProperty); }
		set { SetValue(PasswordProperty, value); }
	}
	
	public BindablePasswordBox()
	{
		InitializeComponent();
		txtPasswordBox.PasswordChanged += OnPasswordChanged;
	}

	/// <summary>
	/// > When the password changes, update the Password property
	/// </summary>
	/// <param name="sender">The object that raised the event.</param>
	/// <param name="RoutedEventArgs">This is the event that is being handled.</param>
	private void OnPasswordChanged(object sender, RoutedEventArgs e)
	{
		Password = txtPasswordBox.SecurePassword;
	}
}