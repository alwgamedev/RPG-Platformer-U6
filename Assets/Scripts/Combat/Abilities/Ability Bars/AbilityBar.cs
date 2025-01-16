using System.Collections.Generic;
using System.Linq;
using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public class AbilityBar
    {
        public const int playerAbilityBarLength = 13;

        private bool subscribedToController;

        List<AbilityBarItem> abilityBarItems = new();
        //Dictionary<AttackAbility, bool> isAbilityOnCooldown = new();
        //Dictionary<AttackAbility, float> cooldownTimers = new();

        public ICombatController CombatController { get; private set; }
        public bool Configured { get; private set; }
        public List<AbilityBarItem> AbilityBarItems => abilityBarItems;
        public Dictionary<AttackAbility, bool> IsAbilityOnCooldown { get; private set; } = new();
        public Dictionary<AttackAbility, float> CooldownTimers { get; private set; } = new();

        public AbilityBar() { }

        public AbilityBar(ICombatController controller, List<AbilityBarItem> abilityBarItems)
        {
            CombatController = controller;
            this.abilityBarItems = abilityBarItems ?? new();
        }

        public AttackAbility GetAbility(int index)
        {
            if(abilityBarItems == null || index < 0 || index >= abilityBarItems.Count) return null;
            return abilityBarItems[index]?.Ability;
        }

        public AttackAbility GetAutoCastAbility()
        {
            foreach (var item in abilityBarItems)
            {
                if (item?.Ability == null) continue;
                if (item.IncludeInAutoCastCycle && !IsAbilityOnCooldown[item.Ability])
                {
                    return item.Ability;
                }
            }
            return null;
        }

        public bool AbilityValid(AttackAbility ability)
        {
            foreach (var entry in IsAbilityOnCooldown)
            {
                if (entry.Key == ability)
                {
                    return true;
                }
            }
            return false;
        }


        public bool IsOnCooldown(AttackAbility ability)
        {
            foreach (var entry in IsAbilityOnCooldown)
            {
                if (entry.Key == ability)
                {
                    return entry.Value;
                }
            }
            return true;
        }

        public async void StartCooldown(AttackAbility ability)
        {
            if (!AbilityValid(ability)) return;
            IsAbilityOnCooldown[ability] = true;
            CooldownTimers[ability] = ability.Cooldown;
            while (CooldownTimers[ability] > 0)
            {
                if (GlobalGameTools.Instance.TokenSource.IsCancellationRequested) return;
                await MiscTools.DelayGameTime(0.06f, GlobalGameTools.Instance.TokenSource.Token);
                if (!CooldownTimers.ContainsKey(ability)) return;
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
            IsAbilityOnCooldown[ability] = false;
            CooldownTimers[ability] = 0;
        }

        public void ResetCooldownTracker()
        {
            foreach (var item in abilityBarItems)
            {
                var ability = item?.Ability;
                if (ability != null)
                {
                    IsAbilityOnCooldown[ability] = false;
                    CooldownTimers[ability] = ability.Cooldown;
                }
            }
        }

        //TO-DO: re-order abilities so that auto-casts are at the beginning of the list
        //or maybe at the END of the list, because you want to use your "good binds" for the non-auto cast abilities
        public void Configure()
        {
            foreach (var item in abilityBarItems)
            {
                var ability = item?.Ability;
                if (ability == null) continue;

                IsAbilityOnCooldown[ability] = false;
                CooldownTimers[ability] = 0;
            }
            if (!subscribedToController && CombatController != null)
            {
                CombatController.OnCooldownStarted += StartCooldown;
                subscribedToController = true;
            }

            Configured = true;
        }

        public void MatchItems(IEnumerable<AbilityBarItem> items)
        {
            abilityBarItems = items.ToList();

            foreach (var item in abilityBarItems)
            {
                var ability = item?.Ability;
                if (item?.Ability == null) continue;

                if (!IsAbilityOnCooldown.ContainsKey(ability))
                {
                    IsAbilityOnCooldown[ability] = false;
                    CooldownTimers[ability] = 0;
                }
            }

            //Keep the old abilities in the cooldown tracker so that you can't reset your cooldown timer
            //exploitatively by removing and adding back the ability on the ability bar!
        }
    }
}