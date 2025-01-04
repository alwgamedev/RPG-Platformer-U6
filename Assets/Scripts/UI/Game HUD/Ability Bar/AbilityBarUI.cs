using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Combat;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class AbilityBarUI : HideableUI
    {
        [SerializeField] AbilityBarItemUI itemPrefab;
        [SerializeField] HorizontalLayoutGroup abilitiesLayoutGroup;

        Dictionary<AttackAbility, AbilityBarItemUI> items = new();
        IAbilityBarOwner player;

        AbilityBar AbilityBar => player?.CurrentAbilityBar;

        protected override void OnEnable()
        {
            base.OnEnable();

            player = FindAnyObjectByType<PlayerCombatController>();
            player.AbilityBarResetEvent += Configure;
            player.OnCooldownStarted += OnCooldownStart;
        }

        public void OnCooldownStart(AttackAbility ability)
        {
            items[ability].StartCooldown();
        }

        public void Configure()
        {
            Clear();
            if (AbilityBar != null)
            {
                foreach (AbilityBarItem abilityBarItem in AbilityBar.abilityBarItems)
                {
                    AbilityBarItemUI item = Instantiate(itemPrefab, abilitiesLayoutGroup.transform);
                    item.Configure(abilityBarItem, AbilityBar.CooldownTimers[abilityBarItem.ability]);
                    items[abilityBarItem.ability] = item;
                    if(item.fillImage.fillAmount > 0)
                    {
                        item.StartCoroutine(item.PlayCooldownAnimation());
                    }
                }
            }
        }

        public void Clear()
        {
            foreach (var item in items)
            {
                Destroy(item.Value.gameObject);
            }
            items.Clear();
        }
    }
}
