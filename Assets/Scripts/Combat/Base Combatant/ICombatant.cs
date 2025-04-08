using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public interface ICombatant : IEquippableCharacter, IDamageDealer
    {
        public string DisplayName { get; }
        public int CombatLevel { get; }
        public float AttackRange { get; }
        public float IdealMinimumCombatDistance { get; } //mainly for AI
        public IWeapon EquippedWeapon { get; }
        public IWeapon DefaultWeapon { get; }
        public IWeapon UnarmedWeapon { get; }
        public ReplenishableStat Stamina { get; }
        public ReplenishableStat Wrath { get; }
        public IHealth Health { get; }

        public event Action OnTargetingFailed;

        public float AdditiveDamageBonus();
        public IHealth FindTarget(Vector2 position, float searchRadius);
        public bool CanAttack(IHealth target);
        public bool CanAttack(float distance, float tolerance);
        public bool TargetInRange(IHealth target);
        public void PrepareProjectile(IProjectile projectile, Func<Vector2> getAimPos, 
            float forceMultiplier, Func<Collider2D, IHealth> hitAction, int maxHits = 1);
        public void PrepareAndShootProjectile(IProjectile projectile, Func<Vector2> getAimPos, 
            float forceMultiplier, Func<Collider2D, IHealth> hitAction, int maxHits = 1);
        public void Attack();
    }
}