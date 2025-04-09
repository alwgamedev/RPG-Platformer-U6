using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IMover : IEntityOrienter
    {
        //public Transform transform { get; }
        public float Width { get; }
        public float Height { get; }

        //public event Action<HorizontalOrientation> DirectionChanged;
    }
}