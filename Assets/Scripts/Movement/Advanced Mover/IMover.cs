using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IMover : IEntityOrienter
    {
        //public Transform transform { get; }
        public Vector3 CenterPosition { get; }//will be collider center rather than transform.position
        public float Width { get; }
        public float Height { get; }

        //public event Action<HorizontalOrientation> DirectionChanged;
    }
}