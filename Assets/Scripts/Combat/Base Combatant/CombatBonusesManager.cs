using UnityEngine;
using RPGPlatformer.Skills;
using RPGPlatformer.Core;

namespace RPGPlatformer.Combat
{
    //[RequireComponent(typeof(CharacterProgressionManager))]
    public class CombatBonusesManager : MonoBehaviour
    {
        //will need references to
        //-the combatant, or at least its IEquippableCharacter
        //-the character's stats (which maybe can be held as a component in Combatant? or we can just getcomponent in awake)

        //Functions:
        //-keep track of active buffs/debuffs from potions, defensive abilities, passive abilities, etc.
        // (or "curses" inflicted on us by enemies)
        //-compute additive damage bonus based on stats and active buffs
        //-compute damage taken based on stats and active buffs
    }
}