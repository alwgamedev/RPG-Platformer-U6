using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [Serializable]
    public struct RandomizableVector2
    {
        [SerializeField] RandomizableMode mode;
        [SerializeField] RandomizableVectorBoundsSource boundsSource;
        [SerializeField] Transform minTransform;
        [SerializeField] Transform maxTransform;
        [SerializeField] Vector2 minPosition;
        [SerializeField] Vector2 maxPosition;

        public Vector2 Value
        {
            get
            {
                if (mode == RandomizableMode.Max)
                {
                    return Max;
                }
                else
                {
                    return MiscTools.RandomPointInRectangle(Min, Max);
                }
            }
        }

        public Vector2 Midpoint => 0.5f * (Min + Max);

        public Vector2 Min =>
            boundsSource == RandomizableVectorBoundsSource.Transform ? minTransform.position : minPosition;

        public Vector2 Max =>
            boundsSource == RandomizableVectorBoundsSource.Transform ? maxTransform.position : maxPosition;
    }
}