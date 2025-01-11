using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public interface ICombatant : IEquippableCharacter, IDamageDealer
    {
        public string DisplayName { get; }
        public int CombatLevel { get; }
        public IWeapon Weapon { get; }
        public ReplenishableStat Stamina { get; }
        public ReplenishableStat Wrath { get; }
        public IHealth Health { get; }

        public event Action OnTargetingFailed;

        public float AdditiveDamageBonus();
        public IHealth FindTarget(Vector2 position, float searchRadius);
        public bool CanAttack(IHealth target);
        public bool CanAttack(float distance);
        public void CheckIfTargetInRange(IHealth target, out bool result);//checks if CanAttack, and, if not, fires an "OnTargetingFailed" event
        public void PrepareProjectile(IProjectile projectile, Vector2 aimPos, float forceMultiplier, Action<Collider2D> hitAction, int maxHits = 1);
        public void PrepareAndShootProjectile(IProjectile projectile, Vector2 aimPos, float forceMultiplier, Action<Collider2D> hitAction, int maxHits = 1);
        public void Attack();
    }
}