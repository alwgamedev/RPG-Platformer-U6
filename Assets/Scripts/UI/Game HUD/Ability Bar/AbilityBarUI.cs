using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGPlatformer.Combat;

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
                slots[i] = slot;
                //if(bar?.AbilityBarItems != null && i < bar.AbilityBarItems.Count)
                //{
                //    var item = bar.AbilityBarItems[i];
                //    slot.PlaceItem(item);
                //    slot.DisplayItem(bar.CooldownTimers[item.Ability]); 
                //    if (AbilityBar.AbilityBarItems[i]?.Ability != null)
                //    {
                //        FindSlot[AbilityBar.AbilityBarItems[i].Ability] = slot;
                //    }
                //    slot.OnDragResolved += UpdateAbilityBar;
                //}
                //else
                //{
                //    slot.DisplayItem(0);
                //}
                //slots[i] = slot;
            }

            //RebuildDictionary();
        }

        public virtual void ConnectAbilityBar(AbilityBar abilityBar, CombatStyle? mainStyle)
        {
            ConnectAbilityBar(abilityBar, DefaultAcceptedCombatStyles(mainStyle));
        }

        public virtual void ConnectAbilityBar(AbilityBar abilityBar, IEnumerable<CombatStyle> acceptedCombatStyles)
        {
            AbilityBar = abilityBar;

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
                    slot.ValidAbility = a => AbilityBar.Abilities == null || !AbilityBar.Abilities.Contains(a);
                }
                else
                {
                    slot.PlaceItem(null);
                    slot.DisplayItem(0);
                }
            }
        }

        public virtual void Clear()
        {
            //to-do (if needed)
        }

        protected void OnCooldownStart(AttackAbility ability)
        {
            FindSlot[ability].StartCooldown();
        }

        //private void RebuildDictionary()
        //{
        //    FindSlot.Clear();
        //    foreach (var slot in slots)
        //    {
        //        var ability = slot.AbilityBarItem?.Ability;
        //        if (ability != null)
        //        {
        //            FindSlot[ability] = slot;
        //        }
        //    }
        //}

        //protected Dictionary<AttackAbility, AbilityBarItemUI> itemLookup = new();
        //protected List<AbilityBarItemUI> allItems = new();//including the empty cells (which aren't in the lookup)
        ////^why this instead of layoutGroup.GetComponentsInChildren<AbilityBarItemUI>()?
        ////because that doesn't work in awake/enable/start when Configure calls Clear
        ////(it doesn't even return a null array, it throws an exception no matter what)
        //protected IAbilityBarOwner player;

        //public AbilityBar AbilityBar { get; private set; }

        //protected override void OnEnable()
        //{
        //    base.OnEnable();

        //    if(usePlayerAbilityBarLength)
        //    {
        //        maxAbilities = AbilityBar.playerAbilityBarLength;
        //    }

        //    if (isPlayerAbilityBar)
        //    {
        //        player = FindAnyObjectByType<PlayerCombatController>();
        //        player.AbilityBarResetEvent += RedrawPlayerAbilityBar;
        //        //^player does this in start so we don't have a race condition
        //        player.OnCooldownStarted += OnCooldownStart;
        //    }
        //}

        //public void OnCooldownStart(AttackAbility ability)
        //{
        //    itemLookup[ability].StartCooldown();
        //}

        //public void RedrawPlayerAbilityBar()
        //{
        //    Clear();
        //    DisplayAbilityBar(player?.CurrentAbilityBar, true);
        //}

        //public void DisplayAbilityBar(AbilityBar abilityBar, bool displayInProgressCooldowns)
        //{
        //    Clear();

        //    AbilityBar = abilityBar;

        //    for (int i = 0; i < maxAbilities; i++)
        //    {
        //        AbilityBarItemUI item = Instantiate(itemPrefab, layoutGroup.transform);
        //        allItems.Add(item);
        //        if (abilityBar != null && i < abilityBar.abilityBarItems.Count)
        //        {
        //            var abilityBarItem = abilityBar.abilityBarItems[i];
        //            if (displayInProgressCooldowns)
        //            {
        //                item.Configure(abilityBarItem, i, abilityBar.CooldownTimers[abilityBarItem.Ability]);
        //                itemLookup[abilityBarItem.Ability] = item;
        //                if (abilityBar.CooldownTimers[abilityBarItem.Ability] > 0)
        //                {
        //                    item.StartCoroutine(item.PlayCooldownAnimation());
        //                }
        //            }
        //            else
        //            {
        //                item.Configure(abilityBarItem, i, 0);
        //            }
        //        }
        //        else
        //        {
        //            item.Configure(null, i, 0);
        //        }
        //    }
        //}

        //public virtual void Clear()
        //{
        //    foreach (var item in allItems)
        //    {
        //        Destroy(item.gameObject);
        //    }
        //    itemLookup?.Clear();
        //    allItems.Clear();
        //}

        //protected override void OnDestroy()
        //{
        //    base.OnDestroy();

        //    if (player != null)
        //    {
        //        player.AbilityBarResetEvent -= RedrawPlayerAbilityBar;
        //    }
        //}
    }
}
