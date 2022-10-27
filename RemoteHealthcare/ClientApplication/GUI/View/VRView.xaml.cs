using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClientApplication.ViewModel;
using ClientSide.VR;
using Shared;

namespace ClientApplication.View;

public partial class VRView : UserControl
{
	private string assetPath = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin", StringComparison.Ordinal)) + "Assets\\";
	public VRView()
	{
		InitializeComponent();
	}

	private void BtnStartVRSession_OnClick(object sender, RoutedEventArgs e)
	{
		Application.Current.Shutdown();
	}

	private void radioButton_Checked(object sender, RoutedEventArgs e)
	{
		var radioButton = (RadioButton)sender; // checked RadioButton

		switch (radioButton.Name)
		{
			case "scene_0":
				// do something
				BitmapImage bitmapScene0 = new BitmapImage();  
				bitmapScene0.BeginInit();
				Console.WriteLine(assetPath);
				bitmapScene0.UriSource = new Uri($"{assetPath}download.png"); 
				bitmapScene0.EndInit();  
				sceneImage.Source = bitmapScene0;
				break;
			
			case "scene_1":
				// do something
				BitmapImage bitmapScene1 = new BitmapImage();
				bitmapScene1.BeginInit();
				Console.WriteLine(assetPath);
				bitmapScene1.UriSource = new Uri($"{assetPath}BikeWallpaper.jpg");
				bitmapScene1.EndInit();
				sceneImage.Source = bitmapScene1;
				break;
			case "scene_2":
				// do something
				break;
			case "scene_3":
				// do something
				break;
			case "scene_4":
				// do something
				break;
			case "scene_5":
				// do something
				break;
			case "scene_random":
				// do something
				BitmapImage bitmapSceneRandom = new BitmapImage();
				bitmapSceneRandom.BeginInit();
				Console.WriteLine(assetPath);
				bitmapSceneRandom.UriSource = new Uri($"{assetPath}randomscene.jpg");
				bitmapSceneRandom.EndInit();
				sceneImage.Source = bitmapSceneRandom;
				break;
			
			case "route_0":
				// do something
				break;
			
			case "route_1":
				// do something
				break;
			case "route_2":
				// do something
				break;
			case "route_3":
				// do something
				break;
			case "route_4":
				// do something
				break;
			case "route_5":
				// do something
				break;
			case "route_random":
				// do something
				break;

		}
	}
}