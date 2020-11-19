using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MewPewServerNew
{
    class User
    {
        public string Id { get; set; }
        public bool Active { get; set; }
        public string Username { get; set; }
        public int Credits { get; set; }
        public PlayerShip Ship { get; set; }
        public Item[] Inventory { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public MewPewSocket Socket { get; set; }
        public Vector3 pos { get; set; }
        public Vector3 rot { get; set; }
        public Vector3 vel { get; set; }
        public Vector3 aVel { get; set; }
        public User(MewPewSocket socket, string username, string id = null)
        {
            Active = true;
            Status = "Hangar";
            Location = "Alpha Centauri";
            Username = username;
            Credits = 10000;
            Socket = socket;
            Inventory = new Item[100];
            if (id == null)
            {
                Id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                Id = Id.Replace("==", String.Empty);
            }
            else
                Id = id;
        }
    }
}
