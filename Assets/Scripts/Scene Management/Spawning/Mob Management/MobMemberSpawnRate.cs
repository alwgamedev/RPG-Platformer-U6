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
        [SerializeField] RandomizableInt maxActivePopulation;
        
        //spawnTimer will not run until active < onlySpawnWhenActiveBelow
        //then it will spawn quantityPerSpawn at interval ticksBetweenSpawns
        //until population is above the threshold again

        public RandomizableFloat QuantityPerSpawn => quantityPerSpawn;
        public RandomizableInt TicksBetweenSpawns => ticksBetweenSpawns;
        public RandomizableInt MaxActivePopulation => maxActivePopulation;
    }
}