using UnityEngine;
using UnityEngine.UI;
using RPGPlatformer.Combat;

namespace RPGPlatformer.UI
{
    public class AbilitiesSettingsTab : SettingsTab
    {
        [SerializeField] Button mageButton;
        [SerializeField] Button meleeButton;
        [SerializeField] Button rangedButton;
        [SerializeField] Button unarmedButton;
        [SerializeField] AbilityBarUI displayBar;

        CombatStyle currentlySelectedCombatStyle;

        //we will need to have player ability bars stored in SettingsManager
        //(add a class AbilitySettingsManager to be held as a component by the SettingsManager)
        //and/or we will definitely need an "AbilitySettingsData" class or struct

        public override void LoadDefaultSettings()
        {
            //display default ability bar for currently selected combat style
        }

        public override void Redraw()
        {
            //should include filling abilityBar with blank abilityBarItemUIs
            //can we do displayBar.Configure(null)?
            //we should adjust the abilityBar class to
            //a) have a bool for whether it is the actual player ability bar
            //b) have a method to display a specific ability bar
        }

        public override bool TrySaveTab(out string resultMessage)
        {
            throw new System.NotImplementedException();
        }
    }
}
