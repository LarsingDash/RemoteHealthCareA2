using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClientSide.VR;

namespace ClientApplication.View;

public partial class VRView : UserControl
{
	public VRView()
	{
		InitializeComponent();
	}

	private void BtnStartVRSession_OnClick(object sender, RoutedEventArgs e)
	{
		Application.Current.Shutdown();
	}
}