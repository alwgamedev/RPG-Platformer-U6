using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGPlatformer.Combat;
using System.Linq;
using System;

namespace RPGPlatformer.UI
{
    public class AbilityBarUI : HidableUI
    {
        [SerializeField] protected AbilityBarSlot slotPrefab;
        [SerializeField] protected LayoutGroup layoutGroup;
        [SerializeField] protected int numSlots = AbilityBar.playerAbilityBarLength;
        //[SerializeField] protected bool isPlayerAbilityBar;
        [SerializeField] protected bool usePlayerAbilityBarLength;

        protected AbilityBarSlot[] slots;
        protected Dictionary<AttackAbility, AbilityBarSlot> FindSlot = new();

        public AbilityBar AbilityBar { get; protected set; }

        public event Action AbilityBarChanged;

        public static IEnumerable<CombatStyle> DefaultAcceptedCombatStyles(CombatStyle? mainStyle)
        {
            return mainStyle switch
            {
                CombatStyle.Mage => new List<CombatStyle>() { CombatStyle.Mage, CombatStyle.Any },
                CombatStyle.Melee => new List<CombatStyle>() { CombatStyle.Melee, CombatStyle.Any },
                CombatStyle.Ranged => new List<CombatStyle>() { CombatStyle.Ranged, CombatStyle.Any },
                CombatStyle.Unarmed => new List<CombatStyle>() { CombatStyle.Unarmed, CombatStyle.Any },
                CombatStyle.Any => new List<CombatStyle>() { CombatStyle.Mage, CombatStyle.Melee,
                    CombatStyle.Ranged, CombatStyle.Unarmed, CombatStyle.Any },
                _ => new List<CombatStyle>()
            };
        }

        protected override void Awake()
        {
            base.Awake();

            if (usePlayerAbilityBarLength)
            {
                numSlots = AbilityBar.playerAbilityBarLength;
            }

            Configure();
        }

        public virtual void InitializeSlots()
        {
            slots = new AbilityBarSlot[numSlots]; 
        }

        public virtual void Configure()
        {
            FindSlot.Clear();
            InitializeSlots();

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = Instantiate(slotPrefab, layoutGroup.transform);
                slot.Configure(i, null);
                slot.OnDragResolved += UpdateAbilityBar;
                slot.OnItemChanged += UpdateAbilityBar;
                slots[i] = slot;
            }

            Canvas.ForceUpdateCanvases();
        }

        public virtual void ConnectAbilityBar(AbilityBar abilityBar, CombatStyle? mainStyle)
        {
            ConnectAbilityBar(abilityBar, DefaultAcceptedCombatStyles(mainStyle));
        }

        public virtual void ConnectAbilityBar(AbilityBar abilityBar, IEnumerable<CombatStyle> acceptedCombatStyles)
        {
            AbilityBar = abilityBar;

            if (AbilityBar != null && !AbilityBar.Configured)
            {
                AbilityBar.Configure();
            }

            foreach(var slot in slots)
            {
                slot.SetAcceptedCombatStyles(acceptedCombatStyles);
            }

            UpdateAbilityBarUI();
        }

        public void UpdateAbilityBar()
        {
            if (AbilityBar == null) return;

            List<AbilityBarItem> items = new();
            foreach (var slot in slots)
            {
                items.Add(slot.AbilityBarItem);
            }

            AbilityBar.MatchItems(items);

            UpdateAbilityBarUI();

            AbilityBarChanged?.Invoke();
        }

        public void UpdateAbilityBarUI()
        {
            FindSlot.Clear();

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (AbilityBar?.AbilityBarItems != null && i < AbilityBar.AbilityBarItems.Count)
                {
                    var item = AbilityBar.AbilityBarItems[i];
                    slot.PlaceItem(item);

                    var ability = item?.Ability;
                    float initialCD = 0;

                    if(ability != null)
                    {
                        FindSlot[ability] = slot;
                        if (AbilityBar.CooldownTimers?.TryGetValue(ability, out var cd) ?? false)
                        {
                            initialCD = cd;
                        }
                    }

                    slot.DisplayItem(initialCD);
                }
                else
                {
                    slot.PlaceItem(null);
                    slot.DisplayItem(0);
                }

                slot.ValidAbility = (a, o) =>
                {
                    if(a == null)
                    {
                        return true;
                    }
                    bool c1 = !FindSlot.ContainsKey(a);
                    bool c2 = o != null && o is AbilityBarSlot oSlot && slots.Contains(oSlot);
                    return c1 || c2;
                };
            }
        }

        public virtual void Clear() { }

        protected void OnCooldownStart(AttackAbility ability)
        {
            FindSlot[ability].StartCooldown();
        }
    }
}
