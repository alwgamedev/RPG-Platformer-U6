using TMPro;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;
using RPGPlatformer.Core;
using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.UI
{
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class CombatantHealthBarCanvas : HidableUI
    {
        [SerializeField] GameObject nameContainer;
        [SerializeField] GameObject healthContainer;
        [SerializeField] TextMeshProUGUI tmp;
        [SerializeField] StatBarItem statBar;
        [SerializeField] float mouseExitNameHideDelay = 1;

        bool inCombat;
        Action MouseEnter;
        Action MouseExit;
        Action Destroyed;

        private void Start()
        {
            var mover = GetComponentInParent<IMover>();
            if(mover != null)
            {
                mover.UpdatedXScale += Unflip;
            }
        }

        public void Configure(ICombatController cc)
        {
            if (cc == null) return;

            cc.Combatant.Health.Stat.statBar = statBar;
            tmp.text = $"{cc.Combatant.DisplayName} (Level {cc.Combatant.CombatLevel})";

            cc.CombatEntered += () =>
                {
                    inCombat = true;
                    ShowAll();
                };
            cc.CombatExited += () =>
            {
                inCombat = false;
                HideAll();
            };

            MouseExit = mouseExitNameHideDelay > 0 ?
                async () => await DelayedNameHide(GlobalGameTools.Instance.TokenSource.Token)
                : HideNameIfNotInCombat;

            HideAll();
        }

        private void Unflip(HorizontalOrientation orientation)
        {
            transform.localScale = new((int)orientation * transform.localScale.x, transform.localScale.y,
                transform.localScale.z);
        }

        public void OnMouseEnter()
            //these are not actually detected by the health bar canvas but called by combat controller
        {
            MouseEnter?.Invoke();
            nameContainer.SetActive(true);
        }

        public void OnMouseExit()
        {
            MouseExit?.Invoke();
        }

        private void ShowAll()
        {
            nameContainer.SetActive(true);
            healthContainer.SetActive(true);
        }

        private void HideAll()
        {
            nameContainer.SetActive(false);
            healthContainer.SetActive(false);
        }

        private void HideNameIfNotInCombat()
        {
            if (!inCombat)
            {
                nameContainer.SetActive(false);
            };
        }

        private async Task DelayedNameHide(CancellationToken token)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            try
            {
                MouseEnter += cts.Cancel;
                Destroyed += cts.Cancel;
                await MiscTools.DelayGameTime(mouseExitNameHideDelay, cts.Token);
                HideNameIfNotInCombat();
            }
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                Destroyed -= cts.Cancel;
                MouseEnter -= cts.Cancel;
            }
        }

        protected override void OnDestroy()
        {
            Destroyed?.Invoke();

            base.OnDestroy();

            Destroyed = null;
            MouseEnter = null;
            MouseExit = null;
        }
    }
}