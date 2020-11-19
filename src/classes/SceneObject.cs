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
