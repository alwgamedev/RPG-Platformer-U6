using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IMover
    {
        public Transform Transform { get; }
        public float Width { get; }
        public float Height { get; }

        public event Action<HorizontalOrientation> UpdatedXScale;
    }
}