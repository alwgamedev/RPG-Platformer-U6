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
        [SerializeField] Transform damagePopupSpawnPoint;
        [SerializeField] DamagePopup damagePopupPrefab;
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
                mover.UpdatedXScale += (orientation) => Unflip(mover.Transform);
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
            cc.HealthChangeEffected += SpawnDamagePopup;

            MouseExit = mouseExitNameHideDelay > 0 ?
                async () => await DelayedNameHide(GlobalGameTools.Instance.TokenSource.Token)
                : HideNameIfNotInCombat;

            HideAll();
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

        public void ShowAll()
        {
            nameContainer.SetActive(true);
            healthContainer.SetActive(true);
        }

        public void HideAll()
        {
            nameContainer.SetActive(false);
            healthContainer.SetActive(false);
        }

        private bool CanSpawnDamagePopup()
        {
            return damagePopupSpawnPoint != null && damagePopupPrefab != null;
        }

        private void SpawnDamagePopup(float damage)
        {
            if (damage > 0 && CanSpawnDamagePopup())
            {
                var popup = Instantiate(damagePopupPrefab, damagePopupSpawnPoint.transform);
                popup.PlayDamageEffect(damage);
                //and I think we can destroy the popup in Animation Event? (or here)
            }
        }

        private void HideNameIfNotInCombat()
        {
            if (!inCombat && nameContainer)
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

        private void Unflip(Transform parent)
        {
            //meaning either your parent is backwards (-1), and you're with it (1), or you're backwards (-1)
            //relative to your parent (1)
            //we should just using the orientation sent by UpdatedXScale,
            //but for some reason this was not always reliable
            if (Mathf.Sign(parent.localScale.x) * Mathf.Sign(transform.localScale.x) < 0)
            {
                transform.localScale = new(-transform.localScale.x, transform.localScale.y,
                transform.localScale.z);
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