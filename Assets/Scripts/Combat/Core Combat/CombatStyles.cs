using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public enum CombatStyle
    {
        Any, Unarmed, Mage, Melee, Ranged, None
        //enum so that you can select weapon class from a drop down menu in Unity
    }


    public static class CombatStyles
    {
        public class None { }//return when weapon class look up fails (nothing will inherit from None)
        public class Any { } //base weapon class; can be referenced when want something to apply to all weapons
        public class Unarmed : Any { }
        public class Mage : Any { }
        public class Melee : Any { }
        public class Ranged : Any { }

        public static CombatStyle GetCombatStyleNameFromType(Type type)
        {
            if(Enum.TryParse(typeof(CombatStyle), type.Name, out var style))
            {
                return (CombatStyle)style;
            }

            return CombatStyle.None;
        }
    }
}