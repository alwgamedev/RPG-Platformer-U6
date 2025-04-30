using RPGPlatformer.Inventory;
using RPGPlatformer.Loot;
using RPGPlatformer.Skills;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public class AICombatant : Combatant
    {
        [SerializeField] protected int combatXPReward = 50; 
        [SerializeField] protected int dropSize;//(max) number of items in generated drop
        [SerializeField] protected DropTable dropTable;
        [SerializeField] protected WeaponSO[] weaponSwapSOs;

        //^in the future to make things more balanced you may want to have separate common and rare drop table,
        //and draw a specific number from each

        protected DamageTakenTracker damageTracker;
        protected Dictionary<string, Weapon> weaponSwaps = new();

        public DamageTakenTracker DamageTracker => damageTracker;
        public int DropSize => dropSize;
        public DropTable DropTable => dropTable;

        protected override void Awake()
        {
            base.Awake();

            damageTracker = new();

            InitializeWeaponSwaps();
        }

        protected void InitializeWeaponSwaps()
        {
            if (unarmedWeapon == null)
            {
                InitializeUnarmedWeapon();
            }

            if (unarmedWeapon != null)
            {
                weaponSwaps[unarmedWeapon.BaseData.LookupName] = unarmedWeapon;
            }

            if (weaponSwapSOs != null)
            {
                foreach (var so in weaponSwapSOs)
                {
                    if (so)
                    {
                        var s = (Weapon)so.CreateInstanceOfItem();
                        if (s != null)
                        {
                            weaponSwaps[s.BaseData.LookupName] = s;
                        }
                    }
                }
            }
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

        public bool TryFindWeaponSwap(string key, out Weapon weapon)
        {
            return weaponSwaps.TryGetValue(key, out weapon);
        }

        public void EquipWeaponSwap(string key)
        {
            if (weaponSwaps.TryGetValue(key, out var w) && w != null)
            {
                EquipItem(w);
            }
        }

        public override void HandleUnequippedItem(EquippableItem item)
        {
            //AI don't unequip to inventory, because their inventory is for their loot drop

            if (item != null && item is Weapon weapon)
            {
                weaponSwaps[weapon.BaseData.LookupName] = weapon;
            }
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