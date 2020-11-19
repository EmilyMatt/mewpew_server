using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MewPewServerNew
{
    class SceneObject
    {
        public string Prefab { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public bool Destroyed { get; set; }
        public bool Indestructible { get; set; }
        public SceneObject()
        {

        }
    }
}
