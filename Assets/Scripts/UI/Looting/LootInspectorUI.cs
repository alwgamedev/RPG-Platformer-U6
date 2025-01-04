using UnityEngine;
using UnityEngine.UI;
using RPGPlatformer.Loot;
using RPGPlatformer.Core;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.UI
{
    public class LootInspectorUI : GridLayoutInventoryUI
    {
        [SerializeField] Button closeButton;
        [SerializeField] Button takeAllButton;

        protected override void OnEnable()
        {
            base.OnEnable();

            closeButton.onClick.AddListener(CloseInspector);

            LootDrop.OnLootSearched += DisplayLootDrop;//(lootDrop) => DisplayLootDrop(lootDrop);
        }

        protected override void Start() 
        {
            GlobalGameTools.Instance.OnPlayerDeath += CloseInspector;

            //yes, empty method has a use here (to ignore the start method from base,
            //which tries to hook up an owner)
        }

        public override void UpdateInventoryUI()
        {
            if(owner == null || owner.Inventory == null || owner.Inventory.IsEmpty())
            {
                CloseInspector();
                return;
            }

            base.UpdateInventoryUI();
        }

        protected override void ConnectOwner(IInventoryOwner owner)
        {
            base.ConnectOwner(owner);

            if(owner is ILootDrop lootDrop)
            {
                takeAllButton.onClick.AddListener(lootDrop.TakeAll);
                lootDrop.OnDropDestroyed += CloseInspector;
                lootDrop.PlayerOutOfRange += PlayerOutOfRangeHandler;
                lootDrop.BeginInspection();
            }
        }

        protected override void DisconnectOwner()
        {
            if(owner != null && owner is ILootDrop lootDrop)
            {
                lootDrop.EndInspection();
                lootDrop.OnDropDestroyed -= CloseInspector;
                lootDrop.PlayerOutOfRange -= PlayerOutOfRangeHandler;
            }

            base.DisconnectOwner();

            takeAllButton.onClick.RemoveAllListeners();
        }

        private void PlayerOutOfRangeHandler()
        {
            if (owner == null || owner is not ILootDrop lootDrop) return;
            CloseInspector();
            lootDrop.PlayerOutOfRange -= PlayerOutOfRangeHandler;
        }

        private void DisplayLootDrop(ILootDrop lootDrop)
        {
            DisconnectOwner();
            ConnectOwner(lootDrop);

            if(!Visible)
            {
                Show();
            }
        }

        private void CloseInspector()
        {
            Hide();
            DisconnectOwner();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            LootDrop.OnLootSearched -= DisplayLootDrop;

            closeButton.onClick.RemoveAllListeners();
            takeAllButton.onClick.RemoveAllListeners();
        }
    }
}