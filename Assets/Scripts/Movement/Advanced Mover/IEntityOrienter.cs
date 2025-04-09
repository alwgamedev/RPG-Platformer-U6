using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IEntityOrienter
    {
        public Transform transform { get; }
        public HorizontalOrientation CurrentOrientation { get; }

        public event Action<HorizontalOrientation> DirectionChanged;
    }
}