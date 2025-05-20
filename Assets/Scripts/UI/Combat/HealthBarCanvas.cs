using RPGPlatformer.Movement;
using TMPro;
using UnityEngine;
using System.Collections;
using RPGPlatformer.Combat;
using RPGPlatformer.Core;

namespace RPGPlatformer.UI
{
    public class HealthBarCanvas : HidableUI
    {
        [SerializeField] protected GameObject nameContainer;
        [SerializeField] protected GameObject healthContainer;
        [SerializeField] protected TextMeshProUGUI tmp;
        [SerializeField] protected StatBarItem statBar;
        [SerializeField] protected Transform damagePopupSpawnPoint;
        [SerializeField] protected DamagePopup damagePopupPrefab;
        [SerializeField] protected float disengageTime = 6;

        protected bool noParentName;
        protected bool healthEngaged;
        //for combatants this will be whether combatant is inCombat
        //for non-combatants this will turn on whenever health takes damage and toggle off after a timer; 
        protected bool healthDead;
        protected float engagementTimer;
        protected IEntityOrienter parentOrienter;

        protected virtual void Update()
        {
            if (healthEngaged)
            {
                engagementTimer += Time.deltaTime;
            }

            if (engagementTimer > disengageTime)
            {
                OnEndEngagement();
            }
        }

        //this is designed for stand-alone (non-combatant) healths that have takeDamageAutomatically = true
        //(e.g. health on a barrier that can be broken down)
        public void Configure(IHealth health)
        {
            if (health == null) return;

            parentOrienter = health.transform.GetComponent<IEntityOrienter>();
            if (parentOrienter != null)
            {
                parentOrienter.DirectionChanged += Unflip;
            }

            health.Stat.statBar = statBar;
            if (health.transform.TryGetComponent(out IDisplayNameSource d))
            {
                tmp.text = $"{d.DisplayName}";
            }
            else
            {
                tmp.text = "";
                noParentName = true;
            }

            health.HealthChangeTrigger += (d, dd) =>
            {
                if (health.IsDead) return;

                if (!healthEngaged)
                {
                    OnBeginEngagement();
                }

                engagementTimer = 0;
                SpawnDamagePopup(d);
            };
            
            
            health.OnDeath += HealthDeathHandler;

            HideAll();
        }

        //public void Configure(ICombatController cc)
        //{
        //    if (cc == null) return;

        //    parentOrienter = cc.Combatant.transform.GetComponent<IEntityOrienter>();
        //    if (parentOrienter != null)
        //    {
        //        parentOrienter.DirectionChanged += Unflip;
        //    }

        //    cc.Combatant.Health.Stat.statBar = statBar;
        //    tmp.text = $"{cc.Combatant.DisplayName} (Level {cc.Combatant.CombatLevel})";

        //    cc.CombatEntered += () =>
        //    {
        //        inCombat = true;
        //        ShowAll();
        //    };
        //    cc.CombatExited += () =>
        //    {
        //        inCombat = false;
        //        StartCoroutine(FadeOut());
        //    };
        //    cc.OnDeath += OnDeath;
        //    cc.HealthChangeEffected += SpawnDamagePopup;

        //    HideAll();
        //}

        protected virtual void OnBeginEngagement()
        {
            healthEngaged = true;
            engagementTimer = 0;
            ShowAll();
        }

        protected virtual void OnEndEngagement()
        {
            healthEngaged = false;
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(FadeOut());
            }
        }

        public void OnMouseEnter()
        //since health bar canvas doesn't have a collider,
        //these need to be called by a parent (whatever health or combat controller we're linked up to)
        {
            if (healthDead || healthEngaged) return;
            ShowNameOnly();
        }

        public void OnMouseExit()
        {
            if (healthDead || healthEngaged || !gameObject.activeInHierarchy) return;
            StartCoroutine(FadeOut(0.5f));
        }

        public void ShowNameOnly()
        {
            if (noParentName) return;

            StopAllCoroutines();
            CanvasGroup.alpha = 1;
            nameContainer.SetActive(true);
        }

        public void ShowAll()
        {
            StopAllCoroutines();
            CanvasGroup.alpha = 1;
            if (!noParentName)
            {
                nameContainer.SetActive(true);
            }
            healthContainer.SetActive(true);
        }

        public IEnumerator FadeOut(float startDelay = 0)
        {
            if (healthDead) yield break;
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

        protected void HealthDeathHandler(IDamageDealer d)
        {
            OnDeath();
            DelayedDestroy();
        }

        protected virtual void OnDeath()
        {
            if (healthEngaged)
            {
                OnEndEngagement();
            }

            healthDead = true;
            transform.SetParent(null);
        }

        protected virtual void DelayedDestroy()
        {
            Destroy(gameObject, 1);
        }

        protected bool CanSpawnDamagePopup()
        {
            return damagePopupSpawnPoint != null && damagePopupPrefab != null;
        }

        protected void SpawnDamagePopup(float damage)
        {
            if (damage < 0 || !CanSpawnDamagePopup()) return;
            var popup = Instantiate(damagePopupPrefab, damagePopupSpawnPoint.transform);
            popup.PlayDamageEffect(damage);
            //popup is destroyed in animation event
        }

        protected void Unflip(HorizontalOrientation orientation)
        {
            if (parentOrienter == null) return;

            //using the parent local scale rather than the given orientation I guess bc it's more reliable
            //to determine if we actually need to flip? (e.g. if parent is doing front-facing anim like freefall, then
            //they won't have actually flipped their scale)
            if (transform.lossyScale.x < 0/*parentOrienter.transform.localScale.x * transform.localScale.x < 0*/)
            {
                var s = transform.localScale;
                s.x = -s.x;
                transform.localScale = s;
            }
        }

        protected override void OnDestroy()
        {
            if (parentOrienter != null)
            {
                parentOrienter.DirectionChanged -= Unflip;
            }

            base.OnDestroy();
        }
    }
}