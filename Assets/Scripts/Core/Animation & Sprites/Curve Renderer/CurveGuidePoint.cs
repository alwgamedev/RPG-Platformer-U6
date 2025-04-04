using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [Serializable]
    public struct CurveGuidePoint
    {
        public int virtualSegments;
        //length units of the curve segment ending in this CurvePoint
        //(to control stretching when points are deactivated)
        public Vector3 point;
        public Vector3 velocity;

        public CurveGuidePoint(int virtualSegments, Vector3 point, Vector3 velocity)
        {
            this.virtualSegments = virtualSegments;
            this.point = point;
            this.velocity = velocity;
        }
    }
}