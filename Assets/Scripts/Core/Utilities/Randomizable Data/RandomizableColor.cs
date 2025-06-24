using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [Serializable]
    public struct RandomizableColor
    {
        [SerializeField] RandomizableMode mode;
        [SerializeField] Color min;
        [SerializeField] Color max;

        public Color Value
        {
            get
            {
                if (mode == RandomizableMode.Max)
                {
                    return max;
                }

                return MiscTools.RandomColor(min, max);
            }
        }

        public Color Min => min;

        public Color Max => max;
    }
}