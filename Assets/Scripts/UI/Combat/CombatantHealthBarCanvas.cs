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
        //Action Destroyed;

        //private void Start()
        //{
        //    parentMover = GetComponentInParent<IMover>();
        //    if(parentMover != null)
        //    {
        //        parentMover.UpdatedXScale += Unflip;
        //    }
            
        //    //popupTargetCanvas = GameObject.Find("Game UI Canvas").GetComponent<Canvas>();
        //}

        public void Configure(ICombatController cc)
        {
            if (cc == null) return;

            parentMover = cc.Combatant.Transform.GetComponent<IMover>();
            parentMover.UpdatedXScale += Unflip;

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

            //MouseExit = mouseExitNameHideDelay > 0 ?
            //    async () => await DelayedNameHide(GlobalGameTools.Instance.TokenSource.Token)
            //    : HideNameIfNotInCombat;

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

            //meaning either your parent is backwards (-1), and you're with it (1), or you're backwards (-1)
            //relative to your parent (1)
            //we should just using the orientation sent by UpdatedXScale,
            //but for some reason this was not always reliable
            if (Mathf.Sign(parentMover.Transform.localScale.x) * Mathf.Sign(transform.localScale.x) < 0)
            {
                transform.localScale = new(-transform.localScale.x, transform.localScale.y,
                transform.localScale.z);
            }
        }

        protected override void OnDestroy()
        {
            //Destroyed?.Invoke();

            if (parentMover != null)
            {
                parentMover.UpdatedXScale -= Unflip;
            }

            base.OnDestroy();

            //Destroyed = null;
        }
    }
}