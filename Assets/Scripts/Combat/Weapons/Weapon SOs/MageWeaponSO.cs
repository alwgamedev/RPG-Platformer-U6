using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    using static CombatStyles;

    [CreateAssetMenu(fileName = "Mage Weapon", menuName = "Equippable Items/Weapons/Mage Weapon")]
    public class MageWeaponSO : WeaponSO
    {
        public override CombatStyle CombatStyle => GetCombatStyleNameFromType(typeof(Mage));
    }
}