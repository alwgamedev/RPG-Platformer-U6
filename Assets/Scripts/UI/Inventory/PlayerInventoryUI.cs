using UnityEngine;
using RPGPlatformer.Inventory;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class PlayerInventoryUI : GridLayoutInventoryUI
    {
        //protected override void Awake()
        //{
        //    base.Awake();

        //    owner = GameObject.FindGameObjectWithTag("Player").GetComponent<IInventoryOwner>();
        //}

        protected override void FindOwner()
        {
            owner = (IInventoryOwner)GlobalGameTools.Instance.Player.Combatant;
        }
    }
}