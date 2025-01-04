using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public interface IHealth
    {
        public bool IsDead { get; }
        public Transform Transform { get; }
        public ReplenishableStat Stat { get; }

        public event Action<float, IDamageDealer> OnDamaged;
        public event Action<float, bool> OnStunned;
        public event Action<IDamageDealer> OnDeath;

        public void ReceiveDamage(float damage, IDamageDealer damageDealer);

        public void ReceiveStun(float duration, bool freezeAnimation);
    }
}