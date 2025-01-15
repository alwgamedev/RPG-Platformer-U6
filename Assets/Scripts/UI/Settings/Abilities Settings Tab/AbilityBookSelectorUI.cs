using System;
using RPGPlatformer.Combat;

namespace RPGPlatformer.UI
{
    public class AbilityBookSelectorUI : TabMenu
    {
        AbilityBookUI abilityBookDisplay;

        protected override void Awake()
        {
            base.Awake();

            abilityBookDisplay = GetComponentInChildren<AbilityBookUI>();
        }

        public override void OpenTab(Tab tab)
        {
            base.OpenTab(tab);

            if (Enum.TryParse(typeof(CombatStyle), tab.Name, out var combatStyle))
            {
                abilityBookDisplay.DisplayBook((CombatStyle)combatStyle);
            }
        }
    }
}