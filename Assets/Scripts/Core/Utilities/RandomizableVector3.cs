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

        public Vector3 Min => 
            boundsSource == RandomizableVectorBoundsSource.Transform ? minTransform.position : minPosition;

        public Vector3 Max =>
            boundsSource == RandomizableVectorBoundsSource.Transform ? maxTransform.position : maxPosition;
    }
}