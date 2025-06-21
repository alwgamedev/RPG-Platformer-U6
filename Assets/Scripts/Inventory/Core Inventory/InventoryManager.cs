using RPGPlatformer.Saving;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Unity.Loading;
using UnityEngine;

namespace RPGPlatformer.Inventory
{
    public class InventoryManager : MonoBehaviour, IInventoryManager, ISavable
    {
        [SerializeField] protected int numSlots = 20;

        protected IInventoryOwner owner;
        protected InventorySlot[] slots;

        public int NumSlots => numSlots;

        public event Action OnInventoryChanged;

        protected virtual void Awake()
        {
            owner = GetComponent<IInventoryOwner>();

            InitializeSlots();
        }

        private void InitializeSlots()
        {
            slots = new InventorySlot[numSlots];
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new InventorySlot();
            }
        }

        public JsonNode CaptureState()
        {
            return JsonSerializer.SerializeToNode(slots.Select(x => x?.ConvertToSerializable()));
        }

        public void RestoreState(JsonNode jNode)
        {
            var ser = jNode.Deserialize<SerializableInventorySlot[]>();
            if (ser != null)
            {
                numSlots = ser.Length;
            }

            InitializeSlots();

            if (ser != null)
            {
                for (int i = 0; i < ser.Length; i++)
                {
                    var item = ser[i].Item?.CreateItem();
                    PlaceInSlotOrDistributeToFirstAvailable(i, item?.ToInventorySlotData(ser[i].Quantity));
                }
            }

            OnInventoryChanged?.Invoke();
        }

        public virtual bool TryAddNewSlot()
        {
            return false;
        }

        public bool ContainsItem(string itemLookup)
        {
            return FindFirstSlotContaining(itemLookup) >= 0;
        }

        public bool ContainsItem(InventoryItem item)
        {
            return FindFirstSlotContaining(item) >= 0;
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if(GetItemInSlot(i) != null)
                {
                    return false;
                }
            }
            return true;
        }

        public InventoryItem GetItemInSlot(int i)
        {
            return slots[i].Item;
        }

        public int GetQuantityInSlot(int i)
        {
            return slots[i].Quantity;
        }

        public IInventorySlotDataContainer GetDataForSlot(int i)
        {
            return slots[i];
        }

        //public IInventorySlotDataContainer[] GetInventory()
        //{
        //    return slots;
        //}

        public void MatchItems(IInventorySlotDataContainer[] data, bool matchSize = false)//needs to allow for quantities
        {
            if (data == null) return;
            if (matchSize)
            {
                numSlots = data.Length;
                InitializeSlots();
                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i] = new InventorySlot();
                    CorePlaceItem(i, data[i]);
                }
            }
            else
            {
                int n = Math.Min(data.Length, slots.Length);
                for (int i = 0; i < n; i++)
                {
                    slots[i].EmptySlot();
                    CorePlaceItem(i, data[i]);
                }
            }
            OnInventoryChanged?.Invoke();
        }

        //if slot is filled, it will place in first available
        public void PlaceInSlotOrDistributeToFirstAvailable(int i, IInventorySlotDataContainer data)
        {
            if (data?.Item == null || data.Quantity == 0) return;

            if (slots[i].Item == null || data.Item.Equals(slots[i].Item))
            {
                DistributeToFirstAvailableSlots(CorePlaceItem(i, data));
                return;
            }
            else
            {
                DistributeToFirstAvailableSlots(data);
            }
        }

        //returns remaining items
        public IInventorySlotDataContainer[] DistributeToFirstAvailableSlots(IInventorySlotDataContainer[] data)
        {
            if(data == null)
            {
                return null;
            }

            IInventorySlotDataContainer[] leftOvers = new IInventorySlotDataContainer[data.Length];
            for(int i = 0; i < data.Length; i++)
            {
                leftOvers[i] = DistributeToFirstAvailableSlots(data[i]);
            }
            return leftOvers;
        }

        public IInventorySlotDataContainer DistributeToFirstAvailableSlots(IInventorySlotDataContainer data)
        {
            if(data?.Item == null || data.Item.BaseData.MaxStack == 0)
            {
                return null;
            }

            int remaining = data.Quantity;
            if (remaining <= 0)
            {
                OnInventoryChanged?.Invoke();//Why? because this method is recursive! Don't delete!
                return null;
            }

            int i = FindFirstVacantSlotOfType(data.Item);
            if(i >= 0)
            {
                var leftOver = CorePlaceItem(i, data);
                if (leftOver.Quantity >= remaining)
                {
                    //juuust in case anything goes wrong, this will make sure we don't get caught
                    //in an infinite loop
                    Debug.LogWarning($"placing item {data.Item.BaseData.DisplayName} failed");
                    Debug.LogWarning($"attempted to distribute {remaining} quantity to inventory slots " +
                        $"and after distributing there is {leftOver.Quantity} quantity remaining " +
                        "(>= starting quantity)");
                    return leftOver;
                }
                return DistributeToFirstAvailableSlots(leftOver);
            }
            else
            {
                int j = FindFirstEmptySlot();
                if(j < 0)
                {
                    OnInventoryChanged?.Invoke();
                    return data;
                }

                var leftOver = CorePlaceItem(j, data);
                if (leftOver.Quantity >= remaining)
                {
                    Debug.LogWarning($"placing item {data.Item.BaseData.DisplayName} failed");
                    Debug.LogWarning($"attempted to distribute {remaining} quantity to inventory slots " +
                        $"and after distributing there is {leftOver.Quantity} quantity remaining");
                    return leftOver;
                }

                return DistributeToFirstAvailableSlots(leftOver);
            }
        }

        public IInventorySlotDataContainer RemoveFromSlot(int i, int quantity = 1)
        {
            var data = CoreRemoveItem(i, quantity);
            OnInventoryChanged?.Invoke();
            return data;
        }

        public IInventorySlotDataContainer RemoveAllFromSlot(int i)
        {
            return RemoveFromSlot(i, slots[i].Quantity);
        }

        public IInventorySlotDataContainer[] RemoveAllItems()
        {
            if(slots == null)
            {
                return null;
            }
            return Enumerable.Range(0, slots.Length).Select(i => RemoveAllFromSlot(i)).ToArray();
        }

        public int RemoveItem(InventoryItem item, int quantity = 1)
        {
            var r = QuietRemoveItem(item, quantity);
            OnInventoryChanged?.Invoke();
            return r;
        }

        public int RemoveItem(string itemLookup, int quantity = 1)
        {
            var r = QuietRemoveItem(itemLookup, quantity);
            OnInventoryChanged?.Invoke();
            return r;
        }

        //remove item without invoking OnInventoryChanged
        private int QuietRemoveItem(InventoryItem item, int quantity = 1)
        {
            if (quantity <= 0 || item == null)
            {
                return 0;
            }

            int i = FindFirstSlotContaining(item);
            if (i < 0)
            {
                return 0;
            }

            var r = CoreRemoveItem(i, quantity).Quantity;
            if (r > 0 && quantity > r)
            {
                r += RemoveItem(item, quantity - r);
            }

            return r;
        }

        //remove item without invoking OnInventoryChanged
        private int QuietRemoveItem(string itemLookup, int quantity = 1)
        {
            if (quantity <= 0 || itemLookup == null)
            {
                return 0;
            }

            int i = FindFirstSlotContaining(itemLookup);
            if (i < 0)
            {
                return 0;
            }

            var r = CoreRemoveItem(i, quantity).Quantity;
            if (r > 0 && quantity > r)
            {
                r += RemoveItem(itemLookup, quantity - r);
            }

            OnInventoryChanged?.Invoke();
            return r;
        }

        private int FindFirstEmptySlot()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Item == null || slots[i].Quantity == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        private int FindFirstVacantSlotOfType(InventoryItem item)
        {
            if (item != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i].HasSpaceForMore() && item.Equals(slots[i].Item))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private int FindFirstSlotContaining(InventoryItem item)
        {
            if (item != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (item.Equals(slots[i].Item) && slots[i].Quantity > 0)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private int FindFirstSlotContaining(string itemLookup)
        {
            if (itemLookup != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (itemLookup.Equals(slots[i].Item?.BaseData.LookupName) && slots[i].Quantity > 0)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        //NOTE: this will replace existing item if not of same type as current item (even if new item is null)
        private IInventorySlotDataContainer CorePlaceItem(int i, IInventorySlotDataContainer data)
        {
            var leftOver = slots[i].PlaceItem(data);
            slots[i].Item?.OnPlacedInInventorySlot(owner, i);
            return leftOver;
        }

        private IInventorySlotDataContainer CoreRemoveItem(int i, int quantity = 1)
        {
            var data = slots[i].Remove(quantity);
            data?.Item?.OnRemovedFromInventorySlot();
            //if it gave us an item copy, then OnRemoved will be null, so either way this works
            if (slots[i].Quantity == 0)
            {
                slots[i].Item?.OnRemovedFromInventorySlot();
                slots[i].EmptySlot();
            }
            return data;
        }

        private void OnDestroy()
        {
            OnInventoryChanged = null;
        }
    }
}
