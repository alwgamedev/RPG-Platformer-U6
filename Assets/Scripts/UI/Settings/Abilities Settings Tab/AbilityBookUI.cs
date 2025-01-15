using UnityEngine;
using RPGPlatformer.Combat;
using System.Linq;

namespace RPGPlatformer.UI
{
    public class AbilityBookUI : AbilityBarUI
    {
        [SerializeField] AbilityBarItemTooltip tooltip;

        public override void Configure()
        {
            base.Configure();
            
            foreach (var slot in slots)
            {
                if (slot is AbilityBookSlot bookSlot)
                {
                    bookSlot.PointerEnter += () => tooltip.Configure(bookSlot.AbilityBarItem?.Ability);
                }
            }
        }

        public void DisplayBook(CombatStyle combatStyle)
        {
            tooltip.Clear();
            AbilityBar = new(null, AbilityTools.GetAllAbilities(combatStyle)
                .Select(x => new AbilityBarItem(x, false)).ToList());

            ConnectAbilityBar(AbilityBar, DefaultAcceptedCombatStyles(combatStyle));
        }

        public override void Clear()
        {
            tooltip.Clear();
        }
    }
}