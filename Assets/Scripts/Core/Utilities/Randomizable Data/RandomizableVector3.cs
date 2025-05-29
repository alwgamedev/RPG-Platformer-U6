using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public enum RandomizableVectorBoundsSource
    {
        Transform, Position
    }

    [Serializable]
    public struct RandomizableVector3
    {
        [SerializeField] RandomizableMode mode;
        [SerializeField] RandomizableVectorBoundsSource boundsSource;
        [SerializeField] Transform minTransform;
        [SerializeField] Transform maxTransform;
        [SerializeField] Vector3 minPosition;
        [SerializeField] Vector3 maxPosition;

        public Vector3 Value
        {
            get
            {
                if (mode == RandomizableMode.Max)
                {
                    return Max;
                }
                else
                {
                    return MiscTools.RandomPointInBox(Min, Max);
                }
            }
        }

        public Vector3 Midpoint => 0.5f * (Min + Max);

        public Vector3 Min => 
            boundsSource == RandomizableVectorBoundsSource.Transform ? minTransform.position : minPosition;

        public Vector3 Max =>
            boundsSource == RandomizableVectorBoundsSource.Transform ? maxTransform.position : maxPosition;

        public bool PointIsInBounds(Vector3 p, Vector3 buffer = default)
        {
            return p.x > Min.x - buffer.x
                && p.x < Max.x + buffer.x
                && p.y > Min.y - buffer.y
                && p.y < Max.y + buffer.y
                && p.z > Min.z - buffer.z
                && p.z < Max.z + buffer.z;
        }
    }
}