namespace ClientSide.VR;

public class PanelController
{
    private VRClient vrClient;
    private Tunnel tunnel;
    
    public PanelController(VRClient vrClient, Tunnel tunnel)
    {
        this.vrClient = vrClient;
        this.tunnel = tunnel;
        
        //Preparing for speedpanel to be created
        vrClient.IDWaitList.Add("speedpanel", NodeID =>
        {
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\PanelDrawText", new Dictionary<string, string>
                {
                    { "_panelid_", NodeID }
                })},
            });
            
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\GetScene", new Dictionary<string, string>())},
            });
        });
        
        //Finding the camera
        tunnel.SendTunnelMessage(new Dictionary<string, string>
        {
            {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Find", new Dictionary<string, string>
            {
                {"_name_", "Camera"}
            })}
        });

        //Sending the message to create speedpanel once the camera has been found
        vrClient.IDSearchList.Add("Camera", NodeID =>
        {
            tunnel.SendTunnelMessage(new Dictionary<string, string>()
            {
                {"\"_data_\"", JsonFileReader.GetObjectAsString("TunnelMessages\\Panel\\AddPanel", new Dictionary<string, string>
                {
                    {"_name_", "speedpanel"},
                    {"_parent_", NodeID}
                })}
            });
        });
    }
}