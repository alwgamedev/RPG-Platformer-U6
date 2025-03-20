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
        public event Action Destroyed;
    }
}