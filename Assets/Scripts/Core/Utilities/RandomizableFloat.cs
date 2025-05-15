using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public enum RandomizableMode
    {
        Random, Max
    }

    [Serializable]
    public struct RandomizableFloat
    {
        [SerializeField] RandomizableMode mode;
        [SerializeField] float min;
        [SerializeField] float max;

        public float Value
        {
            get
            {
                if (mode == RandomizableMode.Max)
                {
                    return max;
                }

                return MiscTools.RandomFloat(min, max);
            }
        }

        public float Min => min;

        public float Max => max;
    }
}