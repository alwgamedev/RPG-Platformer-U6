using System;
using RPGPlatformer.Combat;

namespace RPGPlatformer.UI
{
    public class AbilityBookSelectorUI : TabMenu
    {
        //this will be a tab menu where all the tabs have the same target "content" GO.
        //opening a "tab" will just reconfigure the content to match the combat style.
        //tab.Name must match exactly the string for the CombatStyle it represents
        //(you should really populate the tabs in code then, but this is fine)

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

        public override void CloseAllTabs()
        {
            foreach (var tab in tabs)
            {
                tab.ObscureButton(true);
            }
            openTab = null;
        }
    }
}