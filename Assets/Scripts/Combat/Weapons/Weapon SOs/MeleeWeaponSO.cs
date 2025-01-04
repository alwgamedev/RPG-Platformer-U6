using UnityEngine;

namespace RPGPlatformer.Combat
{
    using static CombatStyles;

    [CreateAssetMenu(fileName = "Melee Weapon", menuName = "Equippable Items/Weapons/Melee Weapon")]
    public class MeleeWeaponSO : WeaponSO
    {
        public override CombatStyle CombatStyle => GetCombatStyleNameFromType(typeof(Melee));
    }
}