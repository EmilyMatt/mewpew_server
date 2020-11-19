using System.Collections.Generic;

namespace MewPewServerNew
{
    class Map
    {
        public string Name { get; set; }
        public SceneObject[] Scenery = new SceneObject[0];
        public List<User> Players = new List<User>();

        public void UpdateShip(Message[] parameters)
        {
            string id = Message.StringValueOfKey(parameters, "i");
            User player = Players.Find(element => element.Id == id);
            player.pos = Message.Vector3ValueOfKey(parameters, "p");
            player.rot = Message.Vector3ValueOfKey(parameters, "p");
            player.vel = Message.Vector3ValueOfKey(parameters, "p");
            player.aVel = Message.Vector3ValueOfKey(parameters, "p");

            foreach(User element in Players)
            {
                if (element == player)
                    continue;

                player.Socket.SendData($"usp?i={id}" +
                        $"&p={Message.ParseVector3(element.pos)}" +
                        $"&r={Message.ParseVector3(element.rot)}" +
                        $"&v={Message.ParseVector3(element.vel)}" +
                        $"&av={Message.ParseVector3(element.aVel)}");
            }
        }

        public void SpawnNewShip(Message[] parameters)
        {
            string id = Message.StringValueOfKey(parameters, "i");
            User player = Players.Find(element => element.Id == id);
            player.pos = Message.Vector3ValueOfKey(parameters, "p");
            player.rot = Message.Vector3ValueOfKey(parameters, "p");
            player.vel = Message.Vector3ValueOfKey(parameters, "p");
            player.aVel = Message.Vector3ValueOfKey(parameters, "p");

            foreach(User element in Players)
            {
                if (element == player || element.Status != "Roam")
                    continue;

                element.Socket.SendData($"spawnnonplayer?" +
                    $"map={Name}" +
                    $"&id={id}" +
                    $"&mesh={player.Ship.Mesh}" +
                    $"&pos={Message.ParseVector3(player.pos)}" +
                    $"&rot={Message.ParseVector3(player.rot)}" +
                    $"&vel={Message.ParseVector3(player.vel)}" +
                    $"&avel={Message.ParseVector3(player.aVel)}");
            }
        }

        public void SendScene(MewPewSocket socket)
        {
            foreach(SceneObject element in Scenery)
            {

            }

            foreach (User element in Players)
            {
                if (element.Id == socket.UserData.Id)
                    continue;

                if(element.Status != "Roam")
                    continue;

                socket.SendData($"spawnnonplayer?" +
                    $"map={Name}" +
                    $"&id={element.Id}" +
                    $"&mesh={element.Ship.Mesh}" +
                    $"&pos={Message.ParseVector3(element.pos)}" +
                    $"&rot={Message.ParseVector3(element.rot)}" +
                    $"&vel={Message.ParseVector3(element.vel)}" +
                    $"&avel={Message.ParseVector3(element.aVel)}");
            }

            socket.SendData($"readytospawn?map={Name}");
        }

        public Map(string name)
        {
            Name = name;
        }
    }
}
