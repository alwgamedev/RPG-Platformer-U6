using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGPlatformer.UI
{
    public class TabMenu : HidableUI
    {
        [SerializeField] protected List<Tab> tabs;

        protected string openTab;
        protected Dictionary<string, Tab> TabLookup = new();

        protected override void Awake()
        {
            base.Awake();

            if (tabs == null || tabs.Count == 0) return;

            foreach(var tab in tabs)
            {
                if (TabLookup.ContainsKey(tab.Name))
                {
                    Debug.Log($"Tab Menu {gameObject.name} has more than one tab named {tab.Name}");
                    continue;
                }

                TabLookup[tab.Name] = tab;
                tab.Button.onClick.AddListener(() => OpenTab(tab));
            }

            OnShow += () => OpenTab(tabs[0]);
        }

        public virtual void OpenTab(Tab tab)
        {
            CloseAllTabs();
            tab.SetOpen(true);
            tab.Button.Select();//LUCKILY Select() will not call Button.onClick
            //(otherwise we'd run into an infinite loop OpenTab -> Button.Select -> Button.onClick -> OpenTab...)
            openTab = tab.Name;
        }

        public virtual void CloseAllTabs()
        {
            foreach(var tab in tabs)
            {
                tab.SetOpen(false);
            }
            openTab = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var b in GetComponentsInChildren<Button>())
            {
                b.onClick.RemoveAllListeners();
            }
        }
    }
}