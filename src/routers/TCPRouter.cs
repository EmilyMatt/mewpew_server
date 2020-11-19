using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MewPewServerNew
{
    class TCPRouter
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
                case "launch":
                    socket.SendData($"launch?map={socket.UserData.Location}");
                    return;
                case "login":
                    DBController.Login(socket, parameters);
                    return;
                case "selectship":
                    DBController.SelectShip(socket, parameters);
                    return;
                case "reqmyshipdata":
                    socket.SendData($"sendmyshipdata?data={JsonConvert.SerializeObject(socket.UserData.Ship)}");
                    return;
                case "maploaded":
                    DBController.ChangeMap(socket, parameters);
                    return;
                case "disconnect":
                    socket.Disconnect();
                    return;
                case "shipspawned":
                    Program.maps.Find(element => element.Name == socket.UserData.Location).SpawnNewShip(parameters);
                    return;
                case "returnpoll":
                    socket.Ping = true;
                    return;
                default:
                    return;
            }
        }
    }
}
