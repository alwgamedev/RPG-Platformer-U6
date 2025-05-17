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
        Transform storedParent;
        Vector3 storedParentLocalPos;

        protected override void Update() { }

        public void Configure(ICombatController cc)
        {
            if (cc == null) return;

            parentOrienter = cc.Combatant.transform.GetComponent<IEntityOrienter>();
            if (parentOrienter != null)
            {
                parentOrienter.DirectionChanged += Unflip;
            }

            cc.Combatant.Health.Stat.statBar = statBar;
            tmp.text = $"{cc.Combatant.DisplayName}\n(Level {cc.Combatant.CombatLevel})";

            cc.CombatEntered += OnBeginEngagement;
            cc.CombatExited += OnEndEngagement;
            cc.OnDeath += CombatDeathHandler(cc);
            cc.HealthChangeEffected += SpawnDamagePopup;

            HideAll();
        }

        private Action CombatDeathHandler(ICombatController cc)
        {
            void Handler()
            {
                storedParent = transform.parent;
                storedParentLocalPos = transform.localPosition;
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
                else
                {
                    transform.SetParent(storedParent);
                    transform.localPosition = storedParentLocalPos;
                }

                cc.Combatant.DeathFinalized -= OnDeathFinalized;
            }

            return Handler;
        }
    }
}