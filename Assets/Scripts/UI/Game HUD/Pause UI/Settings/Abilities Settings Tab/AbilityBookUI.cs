using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Combat;
using System.Linq;

namespace RPGPlatformer.UI
{
    public class AbilityBookUI : AbilityBarUI
    {
        [SerializeField] AbilityBarItemTooltip tooltip;

        public void DisplayBook(CombatStyle combatStyle)
        {
            Clear();

            List<AttackAbility> styleAbilities = AbilityTools.GetAllAbilities(combatStyle).ToList();
            DisplayAbilities(styleAbilities);
        }

        public override void Clear()
        {
            base.Clear();
            tooltip.Clear();
        }

        public void DisplayAbilities(List<AttackAbility> abilities)
        {
            Clear();
            for (int i = 0; i < maxAbilities; i++)
            {
                AbilityBarItemUI item = Instantiate(itemPrefab, layoutGroup.transform);
                allItems.Add(item);
                if (abilities != null && i < abilities.Count)
                {
                    if (abilities[i] == null)
                    {
                        Debug.Log($"ability {i} is null");
                        continue;
                    }
                    AbilityBarItem abilityBarItem = new(abilities[i], false);
                    item.Configure(abilityBarItem, i, 0);
                    if(item is AbilityBookItemUI bookItem)
                    {
                        bookItem.PointerEnter += () => tooltip.Configure(bookItem.AbilityBarItem);
                    }
                }
                else
                {
                    item.Configure(null, i, 0);
                }
            }
        }
    }
}