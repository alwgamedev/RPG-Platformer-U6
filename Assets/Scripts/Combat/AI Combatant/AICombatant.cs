using RPGPlatformer.Skills;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public class AICombatant : Combatant
    {
        [SerializeField] protected int combatXPReward = 50;

        public DamageTakenTracker damageTracker;
        public float MinimumCombatDistance { get; protected set; }

        protected override void Awake()
        {
            base.Awake();

            damageTracker = new();
        }

        public override float HandleHealthChange(float damage, IDamageDealer damageDealer)
        {
            damageTracker.RecordDamage(damage, damageDealer);
            return base.HandleHealthChange(damage, damageDealer);
        }

        public override void InitializeWeaponSOs()
        {
            base.InitializeWeaponSOs();
            MinimumCombatDistance = unarmedWeapon?.WeaponStats.AttackRange / 3 ?? 0.25f;
        }

        public override void OnDeath()
        {
            base.OnDeath();

            DropXPReward();

        }

        public virtual void DropXPReward()
        {
            foreach(var entry in damageTracker.DamageLookup)
            {
                if(entry.Key is Component component && component.gameObject.TryGetComponent(out IXPGainer recipient))
                {
                    XPRewardSystem.AwardXPForCombatKill(combatXPReward, recipient, entry.Value);
                }
            }
        }
    }
}