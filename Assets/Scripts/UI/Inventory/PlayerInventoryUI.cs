using UnityEngine;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.UI
{
    public class PlayerInventoryUI : GridLayoutInventoryUI
    {
        protected override void Awake()
        {
            base.Awake();

            owner = GameObject.FindGameObjectWithTag("Player").GetComponent<IInventoryOwner>();
        }
    }
}