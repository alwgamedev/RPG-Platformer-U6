using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Combat
{
    public class AbilityBar
    {
        public const int playerAbilityBarLength = 10;

        private bool subscribedToController;

        public ICombatController controller;
        public List<AbilityBarItem> abilityBarItems = new();

        public bool Configured { get; private set; }
        public List<AttackAbility> Abilities { get; private set; } = new();
        public Dictionary<AttackAbility, bool> CooldownTracker { get; private set; } = new();
        public Dictionary<AttackAbility, float> CooldownTimers { get; private set; } = new();

        public AbilityBar()
        {
            abilityBarItems = new();
        }

        public AbilityBar(ICombatController controller, List<AbilityBarItem> abilityBarItems)
        {
            this.controller = controller;
            this.abilityBarItems = abilityBarItems?.GroupBy(y => y.ability).Select(z => z.First()).ToList() ?? new();
        }

        public List<AbilityBarItem> GetDefaultAbilityBarItems(CombatStyles.CombatStyle combatStyle)
        {
            return combatStyle switch
            {
                CombatStyles.CombatStyle.Unarmed => UnarmedAbilities.DefaultAbilityBarData(),
                CombatStyles.CombatStyle.Mage => MageAbilities.DefaultAbilityBarData(),
                CombatStyles.CombatStyle.Melee => MeleeAbilities.DefaultAbilityBarData(),
                CombatStyles.CombatStyle.Ranged => RangedAbilities.DefaultAbilityBarData(),
                _ => null
            };
        }

        public AttackAbility GetAbility(int index)
        {
            if(Abilities == null || index < 0 || index >= Abilities.Count) return null;
            return Abilities[index];
        }

        public AttackAbility GetAutoCastAbility()
        {
            try
            {
                return abilityBarItems.FirstOrDefault(x => x.includeInAutoCastCycle && !CooldownTracker[x.ability]).ability;
            }
            catch
            {
                return null;
            }
        }

        public bool AbilityValid(AttackAbility ability)
        {
            return ability != null 
                && (Abilities?.Contains(ability) ?? false) 
                && (CooldownTracker?.TryGetValue(ability, out _) ?? false);
        }


        public bool OnCooldown(AttackAbility ability)
        {
            return !CooldownTracker.TryGetValue(ability, out bool value) || value;
        }

        public bool OnCooldown(int index)
        {
            try
            {
                return OnCooldown(Abilities[index]);
            }
            catch (IndexOutOfRangeException)
            {
                return true;
            }
        }

        public async void StartCooldown(AttackAbility ability)
        {
            if (!AbilityValid(ability)) return;
            CooldownTracker[ability] = true;
            CooldownTimers[ability] = ability.Cooldown;
            while (CooldownTimers[ability] > 0)
            {
                if (GlobalGameTools.Instance.TokenSource.IsCancellationRequested) return;
                await MiscTools.DelayGameTime(0.06f, GlobalGameTools.Instance.TokenSource.Token);
                CooldownTimers[ability] -= 0.06f;
                //NOTE: this may look dumb (to do this instead of just awaiting the entire cooldown),
                //but we do need to keep track of the cooldown's progress (e.g. for when you switch ability bars
                //and need to set the fill amount for cooldown bars that are still in progress)
            }
            EndCooldown(ability);

        }

        public void EndCooldown(AttackAbility ability)
        {
            if (!AbilityValid(ability)) return;
            CooldownTracker[ability] = false;
            CooldownTimers[ability] = 0;
        }

        public void ResetCooldownTracker()
        {
            foreach (AttackAbility ability in Abilities)
            {
                CooldownTracker[ability] = false;
                CooldownTimers[ability] = ability.Cooldown;
            }
        }

        //TO-DO: re-order abilities so that auto-casts are at the beginning of the list
        //or maybe at the END of the list, because you want to use your "good binds" for the non-auto cast abilities
        public void Configure()
        {
            //Debug.Log("Number of ability bar items provided: " + abilityBarItems.Count);
            foreach (var item in abilityBarItems)
            {
                AttackAbility ability = item.ability;
                if (ability != null)
                {
                    Abilities.Add(item.ability);
                    CooldownTracker.Add(item.ability, false);
                    CooldownTimers.Add(item.ability, 0);
                }
                else
                {
                    Debug.Log("Provided ability was null.");
                }
            }
            if (!subscribedToController)
            {
                controller.OnCooldownStarted += StartCooldown;
                subscribedToController = true;
            }
            Configured = true;
        }
    }
}