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

namespace Gui_Login_Window
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = this.UsernameField.Text;
            string password = this.PasswordField.Password;

            if (username == "user" && password == "password")
                MessageBox.Show("Logged in!", "Success");
            else if (username == "User" && password == "Password")
                MessageBox.Show("Logged in!", "Success");
            else
                MessageBox.Show("Invalid username or password.", "Login failed");
        }
    }
}
