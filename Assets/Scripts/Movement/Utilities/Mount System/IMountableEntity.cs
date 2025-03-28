using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IMountableEntity
    {
        public Vector2 LocalGravity { get; }
        public Vector3 VelocitySourceTransformRight { get; }
        public Vector3 Position { get; }
        public Vector2 Velocity { get; }

        public event Action<HorizontalOrientation> DirectionChanged;
        public event Action<IMounter> Mounted;
        public event Action<IMounter> MountStay;
        public event Action<IMounter> Dismounted;
        //public event Action<Vector2> ChangeInVelocity;
        public event Action Destroyed;

        public void EnableTriggerStay(bool val);
    }
}