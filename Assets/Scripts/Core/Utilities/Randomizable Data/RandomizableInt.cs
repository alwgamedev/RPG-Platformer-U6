using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [Serializable]
    public struct RandomizableInt
    {
        [SerializeField] RandomizableMode mode;
        [SerializeField] int min;
        [SerializeField] int max;

        public int Value
        {
            get
            {
                if (mode == RandomizableMode.Random)
                {
                    return MiscTools.rng.Next(min, max + 1);
                }

                return max;
            }
        }

        public int Min => min;
        
        public int Max => max;

        public bool PointIsInBounds(float p, float buffer = 0)
        {
            return p > min - buffer && p < max + buffer;
        }
    }
}