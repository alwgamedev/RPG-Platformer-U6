using System;
using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Inventory;
using System.Collections.Generic;

namespace RPGPlatformer.Loot
{
    [Serializable]
    public struct DropGenerator
    {
        [SerializeField] DropGeneratorEntry[] tables;

        public IEnumerable<IInventorySlotDataContainer[]> GenerateDrop()
        {
            foreach (var t in tables)
            {
                yield return t.GenerateDrop();
            }
        }
    }

    [Serializable]
    public struct DropGeneratorEntry
    {
        [SerializeField] DropTable table;
        [SerializeField] RandomizableInt quantityFromTable;

        public IInventorySlotDataContainer[] GenerateDrop()
        {
            return table.GenerateDrop(quantityFromTable.Value);
        }
    }
}