using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSide.VR.CommandHandlers.TunnelMessages
{
    class AddRoute : CommandHandler
    {
        public void handleCommand(VRClient client, JObject ob)
        {

            PathGen.RenderRoad(client, ob);
        }
    }
}
