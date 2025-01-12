using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Combat;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class AbilityBarUI : HidableUI
    {
        [SerializeField] AbilityBarItemUI itemPrefab;
        [SerializeField] LayoutGroup layoutGroup;

        Dictionary<AttackAbility, AbilityBarItemUI> itemLookup = new();
        List<AbilityBarItemUI> allItems = new();//including the empty ones (which aren't in the look)
        //^why this instead of layoutGroup.GetComponentsInChildren<AbilityBarItemUI>()?
        //because that doesn't work in awake/enable/start when Configure calls Clear
        //(it doesn't even return a null array, it throws an exception no matter what)
        IAbilityBarOwner player;

        AbilityBar AbilityBar => player?.CurrentAbilityBar;

        protected override void OnEnable()
        {
            base.OnEnable();

            player = FindAnyObjectByType<PlayerCombatController>();
            player.AbilityBarResetEvent += Configure;
            //^player does this in start so we don't have a race condition
            player.OnCooldownStarted += OnCooldownStart;
        }

        public void OnCooldownStart(AttackAbility ability)
        {
            itemLookup[ability].StartCooldown();
        }

        public void Configure()
        {
            Clear();
            if (AbilityBar != null)
            {
                for (int i = 0; i < AbilityBar.playerAbilityBarLength; i++)
                {
                    AbilityBarItemUI item = Instantiate(itemPrefab, layoutGroup.transform);
                    allItems.Add(item);
                    if (i < AbilityBar.abilityBarItems.Count)
                    {
                        var abilityBarItem = AbilityBar.abilityBarItems[i];
                        item.Configure(abilityBarItem, i, AbilityBar.CooldownTimers[abilityBarItem.ability]);
                        itemLookup[abilityBarItem.ability] = item;
                        if (AbilityBar.CooldownTimers[abilityBarItem.ability] > 0)
                        {
                            item.StartCoroutine(item.PlayCooldownAnimation());
                        }
                    }
                    else
                    {
                        item.Configure(null, i, 0);
                    }
                }
            }
        }

        public void Clear()
        {
            foreach (var item in allItems)
            {
                Destroy(item.gameObject);
            }
            itemLookup?.Clear();
            allItems.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            player.AbilityBarResetEvent -= Configure;
        }
    }
}
