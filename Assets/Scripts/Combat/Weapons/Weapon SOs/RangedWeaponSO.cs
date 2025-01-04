using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    using static CombatStyles;

    [CreateAssetMenu(fileName = "Ranged Weapon", menuName = "Equippable Items/Weapons/Ranged Weapon")]
    public class RangedWeaponSO : WeaponSO
    {
        public override CombatStyle CombatStyle => GetCombatStyleNameFromType(typeof(Ranged));
    }
}