using System;
using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Inventory;
using RPGPlatformer.UI;
using UnityEngine.EventSystems;

namespace RPGPlatformer.Loot
{
    [RequireComponent(typeof(InventoryManager))]
    public class LootDrop : InteractableGameObject, ILootDrop
    {
        [SerializeField] protected float maxLifeTime = 150;
        //[SerializeField] protected float maxSearchableDistance = 2.5f;
        //[SerializeField] protected string displayName = "Loot Bag";

        protected float lifeTimer = 0;
        protected bool beingInspected;
        //protected Transform playerTransform;
        protected ILooter playerLooter;
        protected InventoryManager inventory;
        protected Action OnUpdate;

        public bool IsPlayer => false;//(part of IInventoryOwner interface)
        public override string ExamineText => "I should search this for loot.";
        public override CursorType CursorType => CursorType.Loot;
        public InventoryManager Inventory => inventory;

        public event Action OnDropDestroyed;
        //public event Action PlayerOutOfRange;

        public static event Action<ILootDrop> OnLootSearched;

        protected override void Awake()
        {
            base.Awake();

            playerLooter = playerTransform.gameObject.GetComponent<ILooter>();
            
            inventory = GetComponent<InventoryManager>();
        }

        private void Start()
        {
            lifeTimer = 0;

            inventory.OnInventoryChanged += HandleInventoryChanged;
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= maxLifeTime)
            {
                DestroyDrop();
            }
            OnUpdate?.Invoke();
        }

        private void HandleInventoryChanged()
        {
            if (inventory.IsEmpty())
            {
                DestroyDrop();
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (!eventData.IsLeftMouseButtonEvent()) return;
            if (GlobalGameTools.PlayerIsInCombat|| !PlayerInRangeWithNotifications()) return;

            TakeAll();
        }

        public void HandleInventoryOverflow(IInventorySlotDataContainer data)
        {
            if (data == null)
            {
                Debug.Log($"Loot drop {gameObject.name} detected an overflow in its loot inventory, but the overflow data was null.");
                return;
            }
            Debug.Log($"Loot drop {gameObject.name} was not able to fit all of the assigned items in its loot inventory.");
        }

        public void HandleInventoryOverflow(IInventorySlotDataContainer[] data)
        {
            if(data == null)
            {
                Debug.Log($"Loot drop {gameObject.name} detected an overflow in its loot inventory, but the overflow data was null.");
                return;
            }
            Debug.Log($"Loot drop {gameObject.name} was not able to fit all of the assigned items in its loot inventory.");
            Debug.Log($"[{data.Length} items omitted.]");
        }

        public void ReleaseFromSlot(int i, int quantity = 1)
        {
            playerLooter.TakeLoot(inventory.RemoveFromSlot(i, quantity));
        }

        public void TakeAll()
        {
            playerLooter.TakeLoot(inventory.RemoveAllItems());
        }

        public void Search()
        {
            GameLog.Log($"Searching {DisplayName} ...");
            OnLootSearched?.Invoke(this);
        }

        public void BeginInspection()
        {
            OnUpdate = () => PlayerInRangeWithNotifications();
            lifeTimer = 0;
        }

        public void EndInspection()
        {
            OnUpdate = null;
        }

        protected override void OnPlayerOutOfRange()
        {
            GameLog.Log($"You're too far away to see the contents of {DisplayName} ...");
        }

        private void DestroyDrop()
        {
            OnDropDestroyed?.Invoke();
            Destroy(gameObject);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnDropDestroyed = null;
            //PlayerOutOfRange = null;
            OnUpdate = null;
        }
    }
}