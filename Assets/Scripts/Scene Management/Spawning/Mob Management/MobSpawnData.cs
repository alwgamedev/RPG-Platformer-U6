using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    [Serializable]
    public class MobSpawnData
    {
        [SerializeField] PoolableObject objectToSpawn;
        [SerializeField] UnityEngine.Object configurationData;//e.g. patrol bounds for ai patroller
        [SerializeField] SpawnPositionData spawnPositionSource;
        //[SerializeField] SpawnQuantityGenerator spawnQuantity;
    }
}