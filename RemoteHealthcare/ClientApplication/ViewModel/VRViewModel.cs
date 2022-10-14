using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
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
	
}