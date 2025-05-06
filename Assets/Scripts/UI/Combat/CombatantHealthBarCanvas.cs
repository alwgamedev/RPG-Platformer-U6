using TMPro;
using UnityEngine;
using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
using RPGPlatformer.SceneManagement;
using System.Collections;

namespace RPGPlatformer.UI
{
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class CombatantHealthBarCanvas : HealthBarCanvas
    {
        //[SerializeField] GameObject nameContainer;
        //[SerializeField] GameObject healthContainer;
        //[SerializeField] TextMeshProUGUI tmp;
        //[SerializeField] StatBarItem statBar;
        //[SerializeField] Transform damagePopupSpawnPoint;
        //[SerializeField] DamagePopup damagePopupPrefab;

        //bool inCombat;
        //bool dead;
        //IEntityOrienter parentOrienter;

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
            cc.OnDeath += OnDeath;
            cc.HealthChangeEffected += SpawnDamagePopup;

            HideAll();
        }

        //public void OnMouseEnter()
        //    //these are not actually detected by the health bar canvas but called by combat controller
        //{
        //    if (dead || inCombat) return;
        //    ShowNameOnly();
        //}

        //public void OnMouseExit()
        //{
        //    if (dead || inCombat) return;
        //    StartCoroutine(FadeOut(0.5f));
        //}

        //public void ShowNameOnly()
        //{
        //    StopAllCoroutines();
        //    CanvasGroup.alpha = 1;
        //    nameContainer.SetActive(true);
        //}

        //public void ShowAll()
        //{
        //    StopAllCoroutines();
        //    CanvasGroup.alpha = 1;
        //    nameContainer.SetActive(true);
        //    healthContainer.SetActive(true);
        //}

        //public IEnumerator FadeOut(float startDelay = 0)
        //{
        //    if (dead) yield break;
        //    yield return new WaitForSeconds(startDelay);
        //    yield return CanvasGroup.FadeOut(0.25f);
        //    HideAll();
        //}

        //public void HideAll()
        //{
        //    StopAllCoroutines();
        //    nameContainer.SetActive(false);
        //    healthContainer.SetActive(false);
        //}

        //private void OnDeath()
        //{
        //    healthDead = true;
        //    transform.SetParent(null, true);
        //    //HideAll(); -> will already be fading out from OnCombatExit
        //    Destroy(gameObject, 1);
        //}

        //private bool CanSpawnDamagePopup()
        //{
        //    return damagePopupSpawnPoint != null && damagePopupPrefab != null;
        //}

        //private void SpawnDamagePopup(float damage)
        //{
        //    if (damage < 0 || !CanSpawnDamagePopup()) return;
        //    var popup = Instantiate(damagePopupPrefab, damagePopupSpawnPoint.transform);
        //    popup.PlayDamageEffect(damage);
        //    //popup is destroyed in animation event
        //}

        //private void Unflip(HorizontalOrientation orientation)
        //{
        //    if (parentOrienter == null) return;

        //    //using the parent local scale rather than the given orientation I guess bc it's more reliable
        //    //to determine if we actually need to flip? (e.g. if parent is doing front-facing anim like freefall, then
        //    //they won't have actually flipped their scale)
        //    if (transform.lossyScale.x < 0/*parentOrienter.transform.localScale.x * transform.localScale.x < 0*/)
        //    {
        //        var s = transform.localScale;
        //        s.x = - s.x;
        //        transform.localScale = s;
        //    }
        //}

        //protected override void OnDestroy()
        //{
        //    if (parentOrienter != null)
        //    {
        //        parentOrienter.DirectionChanged -= Unflip;
        //    }

        //    base.OnDestroy();
        //}
    }
}