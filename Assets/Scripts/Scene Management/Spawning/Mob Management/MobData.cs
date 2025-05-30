using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    [Serializable]
    public struct MobData
    {
        [SerializeField] ObjectPoolData[] poolData;
        [SerializeField] RandomizableVector3 spawnPosition;
        [SerializeField] MobMemberSpawnRate spawnRate;
        [SerializeField] MobSize mobSize;

        public ObjectPoolData[] PoolData => poolData;
        public RandomizableVector3 SpawnPosition => spawnPosition;
        public MobMemberSpawnRate SpawnRate => spawnRate;
        public MobSize MobSize => mobSize;
    }
}