using DoctorApplication.MVVM.ViewModel;
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

namespace ClientApplication.ServerConnection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton== MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// When the minimize button is clicked, the window state is set to minimized
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="RoutedEventArgs">This is the event arguments that are passed to the event handler.</param>
        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// When the user clicks the close button, the application will close
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="RoutedEventArgs">This is the event arguments that are passed to the event handler.</param>
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// If the left mouse button is pressed, then move the window
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="MouseButtonEventArgs">This is the event that is triggered when the mouse button is pressed.</param>
        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}