using System;
using System.Collections.Generic;
using System.Linq;
//using UnityEngine;
using RPGPlatformer.Core;
using Unity.VisualScripting;
//using UnityEditor.Playables;
//using static UnityEditor.Progress;

namespace RPGPlatformer.Combat
{
    public class AbilityBar
    {
        public const int playerAbilityBarLength = 13;

        private bool subscribedToController;

        List<AbilityBarItem> abilityBarItems = new();
        
        //public Dictionary<int, AttackAbility> GetAbility = new();
        //List<AttackAbility> abilities = new();

        public ICombatController Controller { get; private set; }
        public bool Configured { get; private set; }
        public List<AbilityBarItem> AbilityBarItems => abilityBarItems;
        //public List<AttackAbility> Abilities => abilities;
        public Dictionary<AttackAbility, bool> CooldownTracker { get; private set; } = new();
        public Dictionary<AttackAbility, float> CooldownTimers { get; private set; } = new();

        public AbilityBar()
        {
            abilityBarItems = new();
        }

        public AbilityBar(ICombatController controller, List<AbilityBarItem> abilityBarItems)
        {
            Controller = controller;
            this.abilityBarItems = abilityBarItems ?? new();
                /*?.GroupBy(y => y.Ability).Select(z => z.First()).ToList()*/
            //Abilities = new();
            //foreach (var item in AbilityBarItems)
            //{
            //    if(item.Ability != null)
            //    {
            //        Abilities.Add(item.Ability);
            //    }
            //}
        }

        public AttackAbility GetAbility(int index)
        {
            if(abilityBarItems == null || index < 0 || index >= abilityBarItems.Count) return null;
            return abilityBarItems[index]?.Ability;//Abilities[index];
        }

        public AttackAbility GetAutoCastAbility()
        {
            return abilityBarItems?.FirstOrDefault(x => x?.Ability != null && x.IncludeInAutoCastCycle 
                && !CooldownTracker[x.Ability])?.Ability;
            //try
            //{
            //    return AbilityBarItems.FirstOrDefault(
            //        x => x.IncludeInAutoCastCycle && !CooldownTracker[x.Ability]).Ability;
            //}
            //catch
            //{
            //    return null;
            //}
        }

        public bool AbilityValid(AttackAbility ability)
        {
            return ability != null && CooldownTracker.ContainsKey(ability);
        }


        public bool OnCooldown(AttackAbility ability)
        {
            return !CooldownTracker.TryGetValue(ability, out bool value) || value;
        }

        //public bool OnCooldown(int index)
        //{
        //    if (abilityBarItems[index]?.Ability == null) return true;
        //    return OnCooldown(abilityBarItems[index].Ability);
        //    //try
        //    //{
        //    //    return OnCooldown(Abilities[index]);
        //    //}
        //    //catch (IndexOutOfRangeException)
        //    //{
        //    //    return true;
        //    //}
        //}

        public async void StartCooldown(AttackAbility ability)
        {
            if (!AbilityValid(ability)) return;
            CooldownTracker[ability] = true;
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
            CooldownTracker[ability] = false;
            CooldownTimers[ability] = 0;
        }

        public void ResetCooldownTracker()
        {
            foreach (var item in abilityBarItems)
            {
                var ability = item?.Ability;
                if (ability != null)
                {
                    CooldownTracker[ability] = false;
                    CooldownTimers[ability] = ability.Cooldown;
                }
            }
        }

        //TO-DO: re-order abilities so that auto-casts are at the beginning of the list
        //or maybe at the END of the list, because you want to use your "good binds" for the non-auto cast abilities
        public void Configure()
        {
            //Debug.Log("Number of ability bar items provided: " + abilityBarItems.Count);
            foreach (var item in AbilityBarItems)
            {
                var ability = item?.Ability;
                if (ability == null) continue;

                //abilities.Add(ability);
                CooldownTracker.Add(ability, false);
                CooldownTimers.Add(ability, 0);
            }
            if (!subscribedToController && Controller != null)
            {
                Controller.OnCooldownStarted += StartCooldown;
                subscribedToController = true;
            }
            Configured = true;
        }

        public void MatchItems(IEnumerable<AbilityBarItem> items)
        {
            //List<AttackAbility> deletedAbilities = abilityBarItems?.Select(x => x?.Ability).ToList() ?? new(); 

            abilityBarItems = items.ToList();

            foreach (var item in abilityBarItems)
            {
                var ability = item?.Ability;
                if (item?.Ability == null) continue;

                //deletedAbilities.Remove(ability);

                if (!CooldownTracker.ContainsKey(ability))
                {
                    CooldownTracker[ability] = false;
                    CooldownTimers[ability] = 0;
                }
            }

            //Keep the old abilities in the cooldown tracker so that you can't reset your cooldown timer
            //exploitatively by removing and adding back the ability on the ability bar!

            //foreach (var ability in deletedAbilities)
            //{
            //    if (ability != null)
            //    {
            //        CooldownTracker.Remove(ability);
            //        CooldownTimers.Remove(ability);
            //    }
            //}
        }
    }
}