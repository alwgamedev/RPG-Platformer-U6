using RPGPlatformer.Core;
using RPGPlatformer.Skills;
using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public interface ICombatant : IEquippableCharacter, IDamageDealer, IDisplayNameSource
    {
        //public string DisplayName { get; }
        public int CombatLevel { get; }
        public float AttackRange { get; }
        public float IdealMinimumCombatDistance { get; } //mainly for AI
        public IWeapon EquippedWeapon { get; }
        public IWeapon UnarmedWeapon { get; }
        public ReplenishableStat Stamina { get; }
        public ReplenishableStat Wrath { get; }
        public IHealth Health { get; }

        public event Action OnTargetingFailed;
        public event Action DeathFinalized;

        public int GetLevel(CharacterSkill skill);
        public float AdditiveDamageBonus();
        public void BeginDebuff(/*debuff data*/);
        public void SetInvincible(bool val);
        public IHealth FindTarget(Vector2 position, float searchRadius);
        public bool CanAttack(IHealth target);
        public bool CanAttackAtDistSqrd(float distanceSqrd, float tolerance);
        public bool CanAttackAtDistSqrd(float distanceSqrd, float tolerance, float attackRange);
        public bool TargetInRange(IHealth target);
        public void PrepareProjectile(IProjectile projectile, Func<Vector2> getAimPos, 
            float forceMultiplier, Func<Collider2D, IHealth> hitAction, int maxHits = 1);
        public void PrepareAndShootProjectile(IProjectile projectile, Func<Vector2> getAimPos, 
            float forceMultiplier, Func<Collider2D, IHealth> hitAction, int maxHits = 1);
        public void ShootQueuedProjectile();
        public void TriggerCombat();
        public void Instakill();
        public void Revive();
    }
}