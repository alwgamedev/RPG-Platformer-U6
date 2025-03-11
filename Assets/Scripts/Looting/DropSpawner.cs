using UnityEngine;
using System.Linq;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.Loot
{
    using static IInventorySlotDataContainer;

    public class DropSpawner : MonoBehaviour
    {
        [SerializeField] LootDrop dropPrefab;

        public LootDrop SpawnDrop(Vector3 position, IInventorySlotDataContainer[] data)
        {
            if (!CanSpawnDrop()) return null;

            IInventorySlotDataContainer[] trimmed = EnforceMaxStack(Trim(data));
            if (trimmed == null || trimmed.Length == 0) return null;

            LootDrop drop = Instantiate(dropPrefab, position, Quaternion.identity);
            drop.Inventory.MatchItems(trimmed, true);
            return drop;
        }

        public LootDrop SpawnDrop(Vector3 position, IInventorySlotDataContainer data)
        {
            return SpawnDrop(position, new[] { data });
        }

        protected virtual bool CanSpawnDrop()
        {
            return dropPrefab != null;
        }
    }
}