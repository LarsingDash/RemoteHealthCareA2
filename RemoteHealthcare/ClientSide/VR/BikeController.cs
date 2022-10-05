using System.ComponentModel.DataAnnotations;
using System.Globalization;
using ClientSide.Bike;
using DataType = ClientSide.Bike.DataType;

namespace ClientSide.VR;

public class BikeController
{
    private VRClient vrClient;
    private Tunnel tunnel;
    private string bikeId;
    
    public BikeController(VRClient vrClient, Tunnel tunnel)
    {
        this.vrClient = vrClient;
        this.tunnel = tunnel;

        bikeId = null;
        vrClient.IDWaitList.Add("bike", nodeId =>
        {
            bikeId = nodeId;
        });
        
        //TODO: move bike startup code to here
    }

    public void RunController()
    {
        var animationSpeed = 0.0;
        var followSpeed = 0.0;
        
        //Retrieve bike data (speed)
        var bikeData = Program.GetBikeData();
        var speedRaw = bikeData[DataType.Speed].ToString(CultureInfo.InvariantCulture);
        var speed = speedRaw.Substring(0, speedRaw.IndexOf('.') + 2);


        Console.WriteLine($"Current speed: {speed} ");
        //Modify the animation speed based on bike speed
        
        
        //Modify the route follow speed based on bike speed

        //Update VR engine with new speeds
    }
}