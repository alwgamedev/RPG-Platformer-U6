using RPGPlatformer.Inventory;
using System.Linq;

namespace RPGPlatformer.Loot
{
    public class LootDropInventoryManager : InventoryManager
    {
        public override bool TryAddNewSlot()
        {
            slots = slots.Append(new()).ToArray();
            numSlots++;
            return true;
        }
    }
}