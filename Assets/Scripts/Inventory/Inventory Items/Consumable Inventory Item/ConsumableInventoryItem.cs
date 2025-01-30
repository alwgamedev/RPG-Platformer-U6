using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Combat;
using RPGPlatformer.UI;

namespace RPGPlatformer.Inventory
{
    using static UITools;

    [Serializable]
    public class ConsumableInventoryItem : InventoryItem, IDosedItem
    {
        [SerializeField] ConsumableStats stats;

        int dosesRemaining;

        public int Doses => stats.Doses;
        public int DosesRemaining => dosesRemaining;

        public ConsumableInventoryItem(InventoryItemData data, ConsumableStats stats, int dosesRemaining = -1) : base(data)
        {
            this.stats = stats;
            if (dosesRemaining >= 0)
            {
                this.dosesRemaining = dosesRemaining;
            }
            else
            {
                this.dosesRemaining = stats.Doses;
            }
        }

        public override InventoryItem ItemCopy()
        {
            return new ConsumableInventoryItem(baseData, stats, dosesRemaining);
        }

        public void ConsumeDose()
        {
            if(dosesRemaining > 0)
            {
                dosesRemaining--;
            }
        }

        public override void OnPlacedInInventorySlot(IInventoryOwner owner, int slotIndex)
        {
            base.OnPlacedInInventorySlot(owner, slotIndex);

            if (owner is not ICombatant combatant) return;

            OnUse = () =>
            {
                if (dosesRemaining > 0)
                {
                    var data = owner.Inventory.RemoveFromSlot(slotIndex, 1);
                    ((ConsumableInventoryItem)data.Item).ConsumeDose();
                    combatant.Health.ReceiveDamage(-stats.HealthGained, null);
                    if (owner.IsPlayer)
                    {
                        GameLog.Log($"{baseData.DisplayName} healed you for {stats.HealthGained} health.");
                    }


                    if (Doses > 1 && dosesRemaining > 1)
                    {
                        owner.Inventory.DistributeToFirstAvailableSlots(data);
                        if (owner.IsPlayer)
                        {
                            GameLog.Log($"{dosesRemaining - 1} dose(s) remaining.");
                        }
                    }
                }
            };

        }
        //and the base class already sets OnUsed = null when removed from inventory

        public override string TooltipText()
        {
            List<string> lines = new();

            if (stats.HealthGained > 0)
            {
                lines.Add($"<b>Heals:</b> {stats.HealthGained}");
            }

            if(stats.Doses > 1)
            {
                lines.Add($"<b>Doses:</b> {stats.Doses}");
            }

            return FormatLinesOfText(lines);
        }

        protected override void InitializeRightClickActions()
        {
            RightClickActions = new()
            {
                ($"Consume {baseData.DisplayName}", Use)
            };
        }
    }
}