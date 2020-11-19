using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MewPewServerNew
{
    class UDPRouter
    {
        public static void Route(MewPewSocket socket, string msg)
        {
            string[] msgParts = msg.Split('?');

            Message[] parameters = new Message[0];
            if (msgParts.Length > 1 && msgParts[1] != "")
                parameters = Message.Parse(msgParts[1]);

            string route = msgParts[0];

            switch (route)
            {
                case "usp":
                    Program.maps.Find(element => element.Name == socket.UserData.Location).UpdateShip(parameters);
                    return;
                default:
                    return;
            }
        }
    }
}
