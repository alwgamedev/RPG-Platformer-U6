using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    using static CombatStyles;

    public class DamageTakenTracker
    {
        public Dictionary<IDamageDealer, Dictionary<CombatStyle, float>> DamageLookup = new();

        public DamageTakenTracker()
        {
            DamageLookup = new();
        }

        public void ClearTracker()
        {
            DamageLookup.Clear();
        }

        public void RecordDamage(float damage, IDamageDealer damageDealer)
        {
            if (damageDealer == null) return;

            CombatStyle style = damageDealer.CurrentCombatStyle;

            if (!DamageLookup.ContainsKey(damageDealer))
            {
                DamageLookup[damageDealer] = new Dictionary<CombatStyle, float>()
                {
                    [style] = damage
                };
            }
            else if (!DamageLookup[damageDealer].ContainsKey(style))
            {
                DamageLookup[damageDealer][style] = damage;
            }
            else
            {
                DamageLookup[damageDealer][style] += damage;
            }
        }

        public float TotalDamageDealtInStyle(IDamageDealer damageDealer, CombatStyle style)
        {
            if (damageDealer == null || !DamageLookup.ContainsKey(damageDealer))
            {
                return 0;
            }

            return DamageLookup[damageDealer][style];
        }

        public Dictionary<CombatStyle, float> DamageDealtByStyle(IDamageDealer damageDealer)
        {
            if (damageDealer == null || !DamageLookup.ContainsKey(damageDealer)) return null;

            return DamageLookup[damageDealer];
        }

        public static Dictionary<CombatStyle, float> PercentOfTotalDamageDealtByStyle(Dictionary<CombatStyle, float> rawDamages)
        {
            if (rawDamages == null) return null;

            float totalDamage = TotalDamageDealt(rawDamages);

            Dictionary<CombatStyle, float> percentDamages = new();

            foreach(var entry in rawDamages)
            {
                if(totalDamage == 0)
                {
                    percentDamages[entry.Key] = 0;
                }
                else
                {
                    percentDamages[entry.Key] = entry.Value / totalDamage;
                }
            }

            return percentDamages;
        }

        public static float TotalDamageDealt(Dictionary<CombatStyle, float> damagesByStyle)
        {
            if(damagesByStyle == null) return 0;

            float totalDamage = 0;

            foreach (var entry in damagesByStyle)
            {
                totalDamage += entry.Value;
            }

            return totalDamage;
        }
    }
}