using UnityEngine;
using RPGPlatformer.Inventory;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class PlayerInventoryUI : GridLayoutInventoryUI
    {
        //CollapsableUI collapsableUI;
        //Animation highlightAnim;

        protected override void Awake()
        {
            base.Awake();

            //collapsableUI = GetComponent<CollapsableUI>();
            //highlightAnim = collapsableUI.CollapseButton.GetComponent<Animation>();
        }

        protected override void FindOwner()
        {
            owner = (IInventoryOwner)GlobalGameTools.Instance.Player.Combatant;
        }
    }
}