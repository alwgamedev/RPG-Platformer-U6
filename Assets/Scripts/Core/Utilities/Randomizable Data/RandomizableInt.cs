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

        public int Value => MiscTools.rng.Next(min, max + 1);

        public int Min => min;
        
        public int Max => max;
    }
}