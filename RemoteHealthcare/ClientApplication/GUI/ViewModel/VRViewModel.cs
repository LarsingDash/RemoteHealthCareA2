using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClientSide.VR;
using ClientSide.VR2;

namespace ClientApplication.ViewModel;

public class VRViewModel:ViewModelBase
{
	public VRViewModel()
	{
		startVRSession = new ViewModelCommand(startVRSessionExecute);
	}

	private static void startVRSessionExecute(object obj)
	{
		App.GetVrClientInstance().Setup();
	}

	public ICommand startVRSession
	{
		get;
	}

	public static string scene_wallpaper { get; set; }
}