using TMPro;
using UnityEngine;
using System;
using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
using RPGPlatformer.SceneManagement;
using System.Collections;

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

        bool inCombat;
        bool dead;
        IMover parentMover;

        public void Configure(ICombatController cc)
        {
            if (cc == null) return;

            parentMover = cc.Combatant.Transform.GetComponent<IMover>();
            parentMover.DirectionChanged += Unflip;

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
                StartCoroutine(FadeOut());
            };
            cc.OnDeath += OnDeath;
            cc.HealthChangeEffected += SpawnDamagePopup;

            HideAll();
        }

        public void OnMouseEnter()
            //these are not actually detected by the health bar canvas but called by combat controller
        {
            if (dead || inCombat) return;
            ShowNameOnly();
        }

        public void OnMouseExit()
        {
            if (dead || inCombat) return;
            StartCoroutine(FadeOut(0.5f));
        }

        public void ShowNameOnly()
        {
            StopAllCoroutines();
            CanvasGroup.alpha = 1;
            nameContainer.SetActive(true);
        }

        public void ShowAll()
        {
            StopAllCoroutines();
            CanvasGroup.alpha = 1;
            nameContainer.SetActive(true);
            healthContainer.SetActive(true);
        }

        public IEnumerator FadeOut(float startDelay = 0)
        {
            if (dead) yield break;
            yield return new WaitForSeconds(startDelay);
            yield return CanvasGroup.FadeOut(0.25f);
            HideAll();
        }

        public void HideAll()
        {
            StopAllCoroutines();
            nameContainer.SetActive(false);
            healthContainer.SetActive(false);
        }

        private void OnDeath()
        {
            dead = true;
            transform.SetParent(null, true);
            HideAll();
            Destroy(gameObject, 1);
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

        private void Unflip(HorizontalOrientation orientation)
        {
            if (parentMover == null) return;

            if (parentMover.Transform.localScale.x * transform.localScale.x < 0)
            {
                transform.localScale = new(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }

        protected override void OnDestroy()
        {
            if (parentMover != null)
            {
                parentMover.DirectionChanged -= Unflip;
            }

            base.OnDestroy();
        }
    }
}