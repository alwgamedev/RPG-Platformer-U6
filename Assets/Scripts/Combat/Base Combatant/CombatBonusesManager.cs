using RPGPlatformer.Skills;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    //[RequireComponent(typeof(CharacterProgressionManager))]
    public class CombatBonusesManager
    {
        //will need references to
        //-the combatant, or at least its IEquippableCharacter
        //-the character's stats (which maybe can be held as a component in Combatant? or we can just getcomponent in awake)

        //Functions:
        //-keep track of active buffs/debuffs from potions, defensive abilities, passive abilities, etc.
        // (or "curses" inflicted on us by enemies -- not poison can just be implement as a bleed,
        // so we don't need that here)
        //-compute additive damage bonus based on stats and active buffs
        //-compute damage taken based on stats and active buffs

        ICombatant combatant;

        //"STATE DATA" -- active bonuses
        public bool invincible;

        public void BeginDebuff(/*debuff data -- e.g. damage dealt or damage taken modifier + float? duration*/)
        {

        }

        public CombatBonusesManager(ICombatant combatant)
        {
            this.combatant = combatant;
        }

        public float AdditiveDamageBonus()
        {
            float total = 0;
            foreach (var entry in combatant.EquipSlots)
            {
                if (entry.Value != null)
                {
                    total += entry.Value.EquippedItem?.EquippableItemData.DamageBonus ?? 0;
                }
            }

            if (combatant.EquippedWeapon != null)
            {
                total += LevelBasedDamageBonus(combatant.EquippedWeapon.CombatStyle);
            }
            return total;
        }

        public float LevelBasedDamageBonus(CombatStyle combatStyle)
        {
            if (CharacterSkillBook.GetCombatSkill(combatStyle) == null) return 0;
            return 4.5f * combatant.GetLevel(CharacterSkillBook.GetCombatSkill(combatStyle));
            //at max level 40 this gives +180 damage (then times ability's damage multiplier)
        }

        public float DamageTakenMultiplier()
        {
            if (invincible)
            {
                return 0;
            }
            return Mathf.Max(1 - LevelBasedDamageReduction() - (DefenseBonus() / 500), 0);
        }

        public float LevelBasedDamageReduction()
        {
            float defenseProgress = combatant.GetLevel(CharacterSkillBook.Defense)
                / CharacterSkillBook.Defense.XPTable.MaxLevel;
            return 0.1f * defenseProgress;
            //hence at max defense you get 10% universal damage reduction
        }

        public float DefenseBonus()
        {
            float total = 0;
            foreach (var entry in combatant.EquipSlots)
            {
                if (entry.Value != null)
                {
                    total += entry.Value.EquippedItem?.EquippableItemData.DefenseBonus ?? 0;
                }
            }
            return total;
        }
    }
}