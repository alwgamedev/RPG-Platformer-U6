using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public interface IMover
    {
        public Transform Transform { get; }

        public event Action<HorizontalOrientation> UpdatedXScale;
    }
}