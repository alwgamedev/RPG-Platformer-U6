using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    [Serializable]
    public struct WeaponStats
    {
        [SerializeField] float baseDamage;
        [SerializeField] float attackRange;
        [SerializeField][Range(1, 50)] int baseAttackRate;//in ticks

        public float BaseDamage => baseDamage;
        public float AttackRange => attackRange;
        public int BaseAttackRate => baseAttackRate;
    }
}