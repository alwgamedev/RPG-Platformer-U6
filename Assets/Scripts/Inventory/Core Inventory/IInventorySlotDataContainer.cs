using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using RPGPlatformer.UI;
using System;

namespace RPGPlatformer.Inventory
{
    public interface IInventorySlotDataContainer : IInventoryItemHolder
    {
        public int Quantity(); 

        public static IInventorySlotDataContainer[] EnforceMaxStack(IInventorySlotDataContainer[] data)
        {
            if (data == null) return null;

            List<IInventorySlotDataContainer> result = new();

            for(int i = 0; i < data.Length; i++)
            {
                if (data[i]?.Item() == null || data[i].Item().BaseData.MaxStack == 0) continue;

                int maxStack = data[i].Item().BaseData.MaxStack;
                int stacks = data[i].Quantity() / maxStack;
                int remainder = data[i].Quantity() - stacks * maxStack;

                for(int j = 0; j < stacks; j++)
                {
                    result.Add(data[i].Item().ItemCopy().ToSlotData(maxStack));
                }
                if (remainder != 0)
                {
                    result.Add(data[i].Item().ToSlotData(remainder));
                }
            }

            return result.ToArray();
        }

        public static IInventorySlotDataContainer[] Trim(IInventorySlotDataContainer[] data)
        {
            return data?.Where(x => x?.Item() != null && x.Quantity() > 0).ToArray();
        }
    }
}