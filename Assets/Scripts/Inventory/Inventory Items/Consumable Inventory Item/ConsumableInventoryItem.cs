using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Combat;
using RPGPlatformer.UI;
using UnityEditor;

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

        public ConsumableInventoryItem(InventoryItemData data, ConsumableStats stats, int dosesRemaining = -1) 
            : base(data)
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

        public override SerializableInventoryItem ConvertToSerializable()
        {
            var ser = new SerializableConsumableInventoryItem()
            {
                LookupName = baseData.LookupName,
                DosesRemaining = dosesRemaining
            };

            return ser;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj)
                && obj is ConsumableInventoryItem item 
                && item.dosesRemaining == dosesRemaining;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ dosesRemaining;
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

            Use = () =>
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
                            //dosesRemaining - 1 because the remaining doses will be in a new inventory
                            //item that got redistributed to inventory
                            //(e.g. you take a bite of a cookie from a stack of 3 - dose cookies
                            //it produces a new cookie with 2 doses remaining and places it in first available slot)
                        }
                    }
                }
            };

        }

        public override void Examine()
        {
            base.Examine();

            GameLog.Log($"{dosesRemaining} dose(s) remaining.");
        }

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

        public override IEnumerable<(string, Action)> RightClickActions()
        {
            if (Use != null)
            {
                yield return ($"Consume {baseData.DisplayName}", Use);
            }
        }

        //protected override void InitializeRightClickActions()
        //{
        //    RightClickActions = new()
        //    {
        //        ($"Consume {baseData.DisplayName}", Use)
        //    };
        //}
    }
}