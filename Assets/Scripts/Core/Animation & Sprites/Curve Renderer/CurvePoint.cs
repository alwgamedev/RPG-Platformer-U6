using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [Serializable]
    public struct CurvePoint
    {
        public Vector3 point;
        public Vector3 velocity;

        public CurvePoint(Vector3 point, Vector3 velocity)
        {
            this.point = point;
            this.velocity = velocity;
        }
    }
}