using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    [Serializable]
    public struct MobMemberSpawnRate
    {
        [SerializeField] RandomizableFloat quantityPerSpawn;
        [SerializeField] RandomizableInt ticksBetweenSpawns;
        [SerializeField] RandomizableInt onlySpawnWhenBelow;
        [SerializeField] RandomizableInt forceSpawnWhenActiveBelow;
    }
}