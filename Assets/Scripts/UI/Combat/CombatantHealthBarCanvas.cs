using System;
using UnityEngine;
using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.UI
{
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class CombatantHealthBarCanvas : HealthBarCanvas
    {
        const int focusedSortingOrder = -1;
        const int defaultSortingOrder = -2;

        //Canvas canvas;

        Transform storedParent;
        Vector3 storedParentLocalPos;

        static CombatantHealthBarCanvas lastHit;

        public static CombatantHealthBarCanvas LastHit
        {
            get => lastHit;
            protected set
            {
                //we could maybe do lock(lastHit) { ... }, but not necessary for us
                if (lastHit != value)
                {
                    lastHit = value;
                    LastHitChanged?.Invoke();
                }
            }
        }

        public static event Action LastHitChanged;

        protected override void Awake()
        {
            base.Awake();

            canvas = GetComponent<Canvas>();
            canvas.sortingOrder = defaultSortingOrder;

            LastHitChanged += UpdateCanvasSortingOrder;
        }

        protected override void Update() { }

        public void Configure(ICombatController cc)
        {
            health = cc?.Combatant?.Health;
            if (cc == null) return;

            parentOrienter = cc.Combatant.transform.GetComponent<IEntityOrienter>();
            if (parentOrienter != null)
            {
                parentOrienter.DirectionChanged += Unflip;
            }

            cc.Combatant.Health.Stat.statBar = statBar;
            tmp.text = $"{cc.Combatant.DisplayName}\n(Level {cc.Combatant.CombatLevel})";

            cc.CombatEntered += OnBeginEngagement;
            cc.CombatExited += OnEndEngagement;//competing with ondeath?
            cc.OnDeath += CombatantDeathHandler(cc);
            cc.HealthChangeEffected += CombatantHealthChangeHandler(cc);

            Unflip(parentOrienter.CurrentOrientation);
            HideAll();
        }

        private void UpdateCanvasSortingOrder()
        {
            canvas.sortingOrder = LastHit == this ? focusedSortingOrder : defaultSortingOrder;
        }

        private Action<float> CombatantHealthChangeHandler(ICombatController cc)
        {
            void Handler(float d)
            {
                LastHit = this;
                SpawnDamagePopup(d);
            }

            return Handler;
        }

        private Action CombatantDeathHandler(ICombatController cc)
        {
            void Handler()
            {
                storedParent = rectTransform.parent;
                storedParentLocalPos = rectTransform.localPosition;
                cc.Combatant.DeathFinalized += OnDeathFinalized;
                OnDeath();
            }

            void OnDeathFinalized()
            {
                if (cc?.Combatant == null)
                    return;

                if (cc.Combatant.DestroyOnFinalizeDeath)
                {
                    DelayedDestroy();
                }
                else if (storedParent)
                {
                    HideAll();
                    rectTransform.SetParent(storedParent);
                    rectTransform.localPosition = storedParentLocalPos;
                }

                cc.Combatant.DeathFinalized -= OnDeathFinalized;
            }

            return Handler;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            LastHitChanged -= UpdateCanvasSortingOrder;

            if (LastHit == this)
            {
                LastHit = null;
            }
        }
    }
}