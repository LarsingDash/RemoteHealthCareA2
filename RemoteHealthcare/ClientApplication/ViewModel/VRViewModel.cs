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
		var vrClient = new VRClient();
	}

	public ICommand startVRSession
	{
		get;
	}
	
}