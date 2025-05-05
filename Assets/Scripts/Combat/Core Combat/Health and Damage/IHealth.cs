using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public interface IHealth
    {
        public bool IsDead { get; }
        public Transform transform { get; }
        public Transform HitEffectTransform { get; }
        public float TargetingTolerance { get; }
        public ReplenishableStat Stat { get; }

        public event Action<float, IDamageDealer> HealthChangeTrigger;
        public event Action<float, bool> OnStunned;
        public event Action<IDamageDealer> OnDeath;

        public void GainHealth(float health, bool clamped);

        public void ReceiveDamage(float damage, IDamageDealer damageDealer);

        public void ReceiveStun(float duration, bool freezeAnimation);
    }
}