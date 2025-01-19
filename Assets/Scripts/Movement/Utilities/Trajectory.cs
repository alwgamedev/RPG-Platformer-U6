using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public readonly struct Trajectory
    {
        public readonly Vector2 startPoint;
        public readonly float timeToReturnToLevel;
        public readonly Func<float, Vector2> position;

        public Trajectory(Vector2 startPoint, float timeToReturnToLevel, Func<float, Vector2> position)
        {
            this.startPoint = startPoint;
            this.timeToReturnToLevel = timeToReturnToLevel;
            this.position = position;
        }
    }
}