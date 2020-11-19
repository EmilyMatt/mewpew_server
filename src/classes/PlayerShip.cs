using System;

namespace MewPewServerNew
{
    [Serializable()]
    class PlayerShip
    {
        public string UserId { get; set; }
        public string Mesh { get; set; }
        public float Mass { get; set; }
        public float Hull { get; set; }
        public float Shields { get; set; }
        public float MaxSpeed { get; set; }
        public float MaxAfterburnerSpeed { get; set; }
        public float Acceleration { get; set; }
        public float AfterburnerAcceleration { get; set; }
        public float AfterburnerTime { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }
        public float Yaw { get; set; }
        public GunType Guns { get; set; }
        public Module[] Modules {get;set;}

    }

    [Serializable()]
    class Sparrow : PlayerShip
    {
        public Sparrow(string userId)
        {
            UserId = userId;
            Mesh = "Sparrow";
            Mass = 50000;
            Hull = 2500;
            Shields = 1500;
            MaxSpeed = 5;
            MaxAfterburnerSpeed = 2.5f;
            Acceleration = 1.5f;
            AfterburnerAcceleration = 1;
            AfterburnerTime = 1000;
            Pitch = 0.5f;
            Roll = 0.75f;
            Yaw = 0.3f;
            Guns = new GunType();
            Modules = new Module[4];
        }

        public Sparrow(string userId, string mesh, float mass, float hull, float shields, float maxSpeed, float maxAfterSpeed, 
            float acc, float afterAcc, float afterTime, float pitch, float roll, float yaw, GunType guns, Module[] modules)
        {
            UserId = userId;
            Mesh = mesh;
            Mass = mass;
            Hull = hull;
            Shields = shields;
            MaxSpeed = maxSpeed;
            MaxAfterburnerSpeed = maxAfterSpeed;
            Acceleration = acc;
            AfterburnerAcceleration = afterAcc;
            AfterburnerTime = afterTime;
            Pitch = pitch;
            Roll = roll;
            Yaw = yaw;
            Guns = guns;
            Modules = modules;
        }
    }
}
