using RPGPlatformer.Inventory;
using UnityEngine;

namespace RPGPlatformer.Loot
{
    //making these SOs for security and convenience
    //(we can easily re-use drop tables, and we don't have to rely on the drop table
    //being manually entered into a serialized field on a game object with 20 other components)
    [CreateAssetMenu(menuName = "Drop Table", fileName = "New Drop Table")]
    public class DropTable : ScriptableObject
    {
        [SerializeField] DropTableEntry[] table;

        //note some slots will be empty if their roll fails
        public IInventorySlotDataContainer[] GenerateDrop(int numSlots)
        {
            var result = new IInventorySlotDataContainer[numSlots];

            if (table == null || table.Length == 0)
            {
                return result;
            }

            int j;

            for (int i =  0; i < numSlots; i++)
            {
                j = MiscTools.rng.Next(table.Length);
                result[i] = table[j].RollAndGenerateDropItem();
                //^I'm not sure about randomizing j every time -- later we may want
                //to have some number of table entries that are included in every drop
            }

            return result;
        }
    }
}