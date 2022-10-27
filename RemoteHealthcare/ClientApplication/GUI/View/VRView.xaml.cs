using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClientApplication.ViewModel;
using ClientSide.VR;
using ClientSide.VR2;
using Shared;

namespace ClientApplication.View;

public partial class VRView : UserControl
{
	private string assetPath = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin", StringComparison.Ordinal)) + "Assets\\";
	public VRView()
	{
		InitializeComponent();
	}

	/// <summary>
	/// > When the user clicks the "Start VR Session" button, the application will close
	/// </summary>
	/// <param name="sender">The object that raised the event.</param>
	/// <param name="e">This is the event that is being handled.</param>
	private void BtnStartVRSession_OnClick(object sender, RoutedEventArgs e)
	{
		Application.Current.Shutdown();
	}

	/// <summary>
	/// The function is called when a radio button is checked. It then sets the selected scenery and route based on the radio
	/// button that was checked
	/// </summary>
	/// <param name="sender">The object that raised the event.</param>
	/// <param name="e">The event data for the routed event that this delegate is handling.</param>
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
				bitmapScene0.UriSource = new Uri($"{assetPath}scene1.jpg"); 
				bitmapScene0.EndInit();  
				sceneImage.Source = bitmapScene0;

				VRClient.SelectedScenery = 0;
				break;
			
			case "scene_1":
				// do something
				BitmapImage bitmapScene1 = new BitmapImage();
				bitmapScene1.BeginInit();
				Console.WriteLine(assetPath);
				bitmapScene1.UriSource = new Uri($"{assetPath}scene2.jpg");
				bitmapScene1.EndInit();
				sceneImage.Source = bitmapScene1;
				
				VRClient.SelectedScenery = 1;
				break;
			case "scene_2":
				// do something
				BitmapImage bitmapScene2 = new BitmapImage();
				bitmapScene2.BeginInit();
				Console.WriteLine(assetPath);
				bitmapScene2.UriSource = new Uri($"{assetPath}scene3.jpg");
				bitmapScene2.EndInit();
				sceneImage.Source = bitmapScene2;
				VRClient.SelectedScenery = 2;
				break;
			case "scene_3":
				// do something
				BitmapImage bitmapScene3 = new BitmapImage();
				bitmapScene3.BeginInit();
				Console.WriteLine(assetPath);
				bitmapScene3.UriSource = new Uri($"{assetPath}scene4.jpg");
				bitmapScene3.EndInit();
				sceneImage.Source = bitmapScene3;
				
				VRClient.SelectedScenery = 3;
				break;
			case "scene_4":
				// do something
				BitmapImage bitmapScene4 = new BitmapImage();
				bitmapScene4.BeginInit();
				Console.WriteLine(assetPath);
				bitmapScene4.UriSource = new Uri($"{assetPath}scene5.jpg");
				bitmapScene4.EndInit();
				sceneImage.Source = bitmapScene4;
				
				VRClient.SelectedScenery = 4;
				break;
			case "scene_5":
				// do something
				BitmapImage bitmapScene5 = new BitmapImage();
				bitmapScene5.BeginInit();
				Console.WriteLine(assetPath);
				bitmapScene5.UriSource = new Uri($"{assetPath}scene6.jpg");
				bitmapScene5.EndInit();
				sceneImage.Source = bitmapScene5;
				
				VRClient.SelectedScenery = 5;
				break;
			case "scene_random":
				// do something
				BitmapImage bitmapSceneRandom = new BitmapImage();
				bitmapSceneRandom.BeginInit();
				Console.WriteLine(assetPath);
				bitmapSceneRandom.UriSource = new Uri($"{assetPath}randomscene.jpg");
				bitmapSceneRandom.EndInit();
				sceneImage.Source = bitmapSceneRandom;
				
				VRClient.SelectedScenery = 6;
				break;
			
			case "route_0":
				// do something
				VRClient.SelectedRoute = 0;
				break;
			
			case "route_1":
				// do something
				VRClient.SelectedRoute = 1;
				break;
			case "route_2":
				// do something
				VRClient.SelectedRoute = 2;
				break;
			case "route_3":
				// do something
				VRClient.SelectedRoute = 3;
				break;
			case "route_4":
				// do something
				VRClient.SelectedRoute = 4;
				break;
			case "route_5":
				// do something
				VRClient.SelectedRoute = 5;
				break;
			case "route_random":
				// do something
				VRClient.SelectedRoute = 6;
				break;

		}
	}
}