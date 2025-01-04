using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    using static CombatStyles;

    [CreateAssetMenu(fileName = "Weapon", menuName = "Equippable Items/Weapons/New Weapon")]
    public class BlankWeaponSO : WeaponSO
    {
        [SerializeField] CombatStyle weaponClass;
        public override CombatStyle CombatStyle => weaponClass;
    }
}