using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGPlatformer.Combat;

namespace RPGPlatformer.UI
{
    public class AbilityBarUI : HidableUI
    {
        [SerializeField] protected AbilityBarItemUI itemPrefab;
        [SerializeField] protected LayoutGroup layoutGroup;
        [SerializeField] protected int maxAbilities = AbilityBar.playerAbilityBarLength;
        [SerializeField] protected bool isPlayerAbilityBar;
        [SerializeField] protected bool usePlayerAbilityBarLength;

        protected Dictionary<AttackAbility, AbilityBarItemUI> itemLookup = new();
        protected List<AbilityBarItemUI> allItems = new();//including the empty cells (which aren't in the lookup)
        //^why this instead of layoutGroup.GetComponentsInChildren<AbilityBarItemUI>()?
        //because that doesn't work in awake/enable/start when Configure calls Clear
        //(it doesn't even return a null array, it throws an exception no matter what)
        protected IAbilityBarOwner player;

        public AbilityBar AbilityBar { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();

            if(usePlayerAbilityBarLength)
            {
                maxAbilities = AbilityBar.playerAbilityBarLength;
            }

            if (isPlayerAbilityBar)
            {
                player = FindAnyObjectByType<PlayerCombatController>();
                player.AbilityBarResetEvent += RedrawPlayerAbilityBar;
                //^player does this in start so we don't have a race condition
                player.OnCooldownStarted += OnCooldownStart;
            }
        }

        public void OnCooldownStart(AttackAbility ability)
        {
            itemLookup[ability].StartCooldown();
        }

        public void RedrawPlayerAbilityBar()
        {
            Clear();
            DisplayAbilityBar(player?.CurrentAbilityBar, true);
        }

        public void DisplayAbilityBar(AbilityBar abilityBar, bool displayInProgressCooldowns)
        {
            Clear();

            AbilityBar = abilityBar;

            for (int i = 0; i < maxAbilities; i++)
            {
                AbilityBarItemUI item = Instantiate(itemPrefab, layoutGroup.transform);
                allItems.Add(item);
                if (abilityBar != null && i < abilityBar.abilityBarItems.Count)
                {
                    var abilityBarItem = abilityBar.abilityBarItems[i];
                    if (displayInProgressCooldowns)
                    {
                        item.Configure(abilityBarItem, i, abilityBar.CooldownTimers[abilityBarItem.ability]);
                        itemLookup[abilityBarItem.ability] = item;
                        if (abilityBar.CooldownTimers[abilityBarItem.ability] > 0)
                        {
                            item.StartCoroutine(item.PlayCooldownAnimation());
                        }
                    }
                    else
                    {
                        item.Configure(abilityBarItem, i, 0);
                    }
                }
                else
                {
                    item.Configure(null, i, 0);
                }
            }
        }

        public virtual void Clear()
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

            if (player != null)
            {
                player.AbilityBarResetEvent -= RedrawPlayerAbilityBar;
            }
        }
    }
}
