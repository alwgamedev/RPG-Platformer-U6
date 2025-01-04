using RPGPlatformer.Combat;
using System;
using UnityEngine;

namespace RPGPlatformer.Inventory
{
    [Serializable]
    public struct ConsumableStats
    {
        [SerializeField] float healthGained;
        [SerializeField][Range(1, 12)] int doses;

        public float HealthGained => healthGained;
        public int Doses => doses;
    }
}