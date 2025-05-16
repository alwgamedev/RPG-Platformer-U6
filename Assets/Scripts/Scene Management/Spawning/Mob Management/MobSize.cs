using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public struct MobSize
    {
        public enum MobSizeCountingOption
        {
            onRelease, onReturn
        }

        [SerializeField] bool hasMax;
        [SerializeField] int maxToSpawn;
        [SerializeField] MobSizeCountingOption countingOption;
    }
}