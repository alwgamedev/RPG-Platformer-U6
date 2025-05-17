using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    [Serializable]
    public struct MobSize
    {
        public enum MobSizeCountingOption
        {
            onRelease, onReturn
        }

        [SerializeField] bool hasMax;
        [SerializeField] int maxToSpawn;
        [SerializeField] MobSizeCountingOption countingOption;

        public bool HasMax => hasMax;
        public int MaxToSpawn => maxToSpawn;
        public MobSizeCountingOption CountingOption => countingOption;
    }
}