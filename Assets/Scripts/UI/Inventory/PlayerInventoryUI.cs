using UnityEngine;
using RPGPlatformer.Inventory;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class PlayerInventoryUI : GridLayoutInventoryUI
    {
        public CollapsableUI CollapsableUI { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            CollapsableUI = GetComponent<CollapsableUI>();
        }

        protected override void FindOwner()
        {
            owner = (IInventoryOwner)GlobalGameTools.Instance.Player.Combatant;
        }
    }
}