using RPGPlatformer.Loot;
using RPGPlatformer.Skills;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public class AICombatant : Combatant
    {
        [SerializeField] protected int combatXPReward = 50; 
        [SerializeField] protected int dropSize;//(max) number of items in generated drop
        [SerializeField] protected DropTable dropTable;

        protected DamageTakenTracker damageTracker;

        public DamageTakenTracker DamageTracker => damageTracker;
        public int DropSize => dropSize;
        public DropTable DropTable => dropTable;

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