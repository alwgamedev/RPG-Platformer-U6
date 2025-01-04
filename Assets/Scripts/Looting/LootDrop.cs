using System;
using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Inventory;
using RPGPlatformer.UI;

namespace RPGPlatformer.Loot
{
    [RequireComponent(typeof(InventoryManager))]
    public class LootDrop : InteractableGameObject, ILootDrop
    {
        [SerializeField] protected float maxLifeTime = 60;
        [SerializeField] protected float maxSearchableDistance = 2.5f;
        [SerializeField] protected string displayName = "Loot Bag";

        protected float lifeTimer = 0;
        protected bool beingInspected;
        protected Transform playerTransform;
        protected ILooter player;
        protected InventoryManager inventory;
        protected Action OnUpdate;

        public bool IsPlayer => false;
        public InventoryManager Inventory => inventory;
        public string DisplayName => $"<b>{displayName}</b>";

        public event Action OnDropDestroyed;
        public event Action PlayerOutOfRange;

        public static event Action<ILootDrop> OnLootSearched;

        private void Awake()
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            playerTransform = playerGO.transform;
            player = playerGO.GetComponent<ILooter>();
            
            inventory = GetComponent<InventoryManager>();
        }

        private void OnEnable()
        {
            lifeTimer = 0;

            inventory.OnInventoryChanged += () =>
            {
                if (inventory.IsEmpty())
                {
                    DestroyDrop();
                }
            };
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
            player.TakeLoot(inventory.RemoveFromSlot(i, quantity));
        }

        public void TakeAll()
        {
            player.TakeLoot(inventory.RemoveAllItems());
        }

        public void Search()
        {
            GameLog.Log($"Searching {DisplayName} ...");
            OnLootSearched?.Invoke(this);
        }

        public void Examine()
        {
            GameLog.Log("I should search this for loot.");
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

        private bool PlayerInRangeWithNotifications()
        {
            if (!PlayerInRange())
            {
                LogPlayerOutOfRange();
                PlayerOutOfRange?.Invoke();
                return false;
            }
            return true;
        }

        private bool PlayerInRange()
        {
            if(player == null)
            {
                return false;
            }

            return Vector2.Distance(playerTransform.position, transform.position) < maxSearchableDistance;
        }

        private void OnMouseDown()
        {
            if (GlobalGameTools.PlayerIsDead) return;
            if (!PlayerInRangeWithNotifications()) return;

            TakeAll();
        }

        private void LogPlayerOutOfRange()
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
            PlayerOutOfRange = null;
            OnUpdate = null;
        }
    }
}