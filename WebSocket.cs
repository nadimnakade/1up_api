using Microsoft.Web.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PickupAPi
{
    public class DialerWebSocket : WebSocketHandler
    {
        private static WebSocketCollection lstEmployees = new WebSocketCollection();
        private string name;
        public DialerWebSocket() { 
            
        }
        public override void OnOpen()
        {
            name = this.WebSocketContext.QueryString["chatName"];
            


            switch (name)
            {
                case "Log":
                    lstEmployees.Broadcast("Its Log time.");
                    break;
                case "Broadcast":
                    lstEmployees.Broadcast("Its Broadcast time.");
                    break;
                default:
                    break;
            }







            lstEmployees.Add(this);
            lstEmployees.Broadcast(name + " has connected.");
        }

        public override void OnMessage(byte[] message)
        {
            lstEmployees.Broadcast(string.Format("{0} said: {1}",name,message)); 
            base.OnMessage(message);
        }

        public override void OnClose()
        {
            lstEmployees.Remove(this);
            lstEmployees.Broadcast(string.Format("{0} has gone away", name));            
        }

    }
}