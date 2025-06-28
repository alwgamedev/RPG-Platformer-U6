using UnityEngine;
using RPGPlatformer.Inventory;
using System.Collections.Generic;

namespace RPGPlatformer.Loot
{
    using static IInventorySlotDataContainer;

    public class DropSpawner : MonoBehaviour
    {
        [SerializeField] LootDrop dropPrefab;
        //[SerializeField] bool combineWithNearestDrop;
        //[SerializeField] float combineDistSqrd = 1;

        //List<LootDrop> activeDrops = new();

        public LootDrop SpawnDrop(Vector3 position, IInventorySlotDataContainer[] data)
        {
            if (!CanSpawnDrop()) return null;

            IInventorySlotDataContainer[] trimmed = EnforceMaxStack(Trim(data));
            if (trimmed == null || trimmed.Length == 0) return null;

            //if (combineWithNearestDrop)
            //{
            //    trimmed = CombineWithNearestLootDrops(trimmed);
            //    trimmed = EnforceMaxStack(Trim(trimmed));
            //    if (trimmed == null || trimmed.Length == 0)
            //    {
            //        if (transform == GlobalGameTools.Instance.PlayerTransform)
            //        {
            //            GameLog.Log("Your dropped items have been combined with an existing drop nearby.");
            //        }
            //        return null;
            //    }
            //}

            LootDrop drop = Instantiate(dropPrefab, position, Quaternion.identity);
            drop.Inventory.MatchItems(trimmed, true);
            //activeDrops.Add(drop);
            //drop.OnDropDestroyed += OnDropDestroyed;
            return drop;
        }

        public LootDrop SpawnDrop(Vector3 position, IInventorySlotDataContainer data)
        {
            return SpawnDrop(position, new[] { data });
        }

        //protected IInventorySlotDataContainer[] CombineWithNearestLootDrops(IInventorySlotDataContainer[] data)
        //{
        //    foreach (var d in activeDrops)
        //    {
        //        if (data == null || data.Length == 0)
        //        {
        //            return data;
        //        }
        //        if (d && Vector2.SqrMagnitude(d.transform.position - transform.position) < combineDistSqrd)
        //        {
        //            data = d.Inventory.DistributeToFirstAvailableSlots(data);
        //        }
        //    }

        //    return data;
        //}

        protected virtual bool CanSpawnDrop()
        {
            return dropPrefab != null;
        }

        //private void OnDropDestroyed(ILootDrop d)
        //{
        //    if (d == null) return;

        //    d.OnDropDestroyed -= OnDropDestroyed;
        //    if (d is LootDrop l)
        //    {
        //        activeDrops.Remove(l);
        //    }
        //}

        //private void OnDestroy()
        //{
        //    foreach (var d in activeDrops)
        //    {
        //        if (d)
        //        {
        //            d.OnDropDestroyed -= OnDropDestroyed;
        //        }
        //    }
        //}
    }
}