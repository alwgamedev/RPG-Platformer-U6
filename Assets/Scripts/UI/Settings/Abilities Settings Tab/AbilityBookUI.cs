using UnityEngine;
using RPGPlatformer.Combat;
using System.Linq;

namespace RPGPlatformer.UI
{
    public class AbilityBookUI : AbilityBarUI
    {
        [SerializeField] AbilityBarItemTooltip tooltip;

        CombatStyle? storedCombatStyle;

        private void Start()
        {
            if (storedCombatStyle.HasValue)
            {
                DisplayBook(storedCombatStyle.Value);
            }
        }

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
            if (!didAwake)
            {
                storedCombatStyle = combatStyle;
            }
            else
            {
                tooltip.Clear();
                var a = new AbilityBar(null, AbilityTools.GetAllAbilities(combatStyle)
                    .Select(x => new AbilityBarItem(x, false)).ToList());

                ConnectAbilityBar(a, DefaultAcceptedCombatStyles(combatStyle));
                storedCombatStyle = null;
            }
        }

        public override void Clear()
        {
            tooltip.Clear();
        }
    }
}