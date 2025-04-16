using UnityEngine;
using RPGPlatformer.UI;
using RPGPlatformer.Saving;
using System.Text.Json.Nodes;
using System.Text.Json;
using RPGPlatformer.Inventory;

namespace RPGPlatformer.Combat
{
    public class PlayerCombatant : Combatant, ISavable
    {
        public override bool IsPlayer => true;

        protected override void ConfigureReplenishableStats()
        {
            var healthBar = GameObject.Find("Player Health Bar");
            var staminaBar = GameObject.Find("Player Stamina Bar");
            var wrathBar = GameObject.Find("Player Wrath Bar");

            if (healthBar)
            {
                Health.Stat.statBar = healthBar.GetComponent<StatBarItem>();
            }
            if (staminaBar)
            {
                Stamina.statBar = staminaBar.GetComponent<StatBarItem>();
            }
            if (wrathBar)
            {
                Wrath.statBar = wrathBar.GetComponent<StatBarItem>();
            }

            base.ConfigureReplenishableStats();
        }

        public override void OnEquipmentLevelReqFailed()
        {
            GameLog.Log("You do not meet the level requirements to equip that item.");
        }

        public JsonNode CaptureState()
        {
            var jNode = new JsonObject();

            foreach (var entry in equipSlots)
            {
                if (!entry.Value) continue;

                jNode.Add(entry.Key.ToString(), 
                    JsonSerializer.SerializeToNode(entry.Value.EquippedItem?.ConvertToSerializable()));
            }

            return jNode;
        }

        //why set the default item instead of just equipping the item?
        //becuase the cc will ask the combatant to equip its default items in start,
        //and we don't know whether that happens before or after restore state completes
        //(and we want this to be as flexible as possible, i.e. removing the "EquipDefaultItems"
        //from cc start to get it to work with save system is not an appealing option)

        //you could add this ISavable implementation to any combatant without issue;
        //for now onlyl doing player to keep the save file small
        public void RestoreState(JsonNode jNode)
        {
            //var data = jNode.Deserialize<SerializableInventoryItem[]>();

            foreach (var entry in equipSlots)
            {
                if (!entry.Value) continue;

                var serItem = jNode[entry.Key.ToString()]?.Deserialize<SerializableInventoryItem>();
                entry.Value.defaultItem = (EquippableItem)(serItem?.CreateItem());
            }

            EquipDefaultItems();
        }
    }
}