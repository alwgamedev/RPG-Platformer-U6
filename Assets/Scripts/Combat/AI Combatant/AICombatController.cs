using RPGPlatformer.UI;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public class AICombatController : CombatController
    {
        protected CombatantHealthBarCanvas healthBarCanvas;

        public IHealth currentTarget;

        public CombatStateManager CombatManager => combatManager;

        protected override void Awake()
        {
            base.Awake();

            healthBarCanvas = GetComponentInChildren<CombatantHealthBarCanvas>();
            if (healthBarCanvas != null)
            {
                healthBarCanvas.Configure(this);
            }
        }

        public void FireOneShot()
        {
            Combatant.CheckIfTargetInRange(currentTarget, out _);
            RunAutoAbilityCycle(false);
        }

        public void StartAttacking()
        {
            FireOneShot();
            CombatManager.OnWeaponTick += FireOneShot;
            combatant.Attack();
        }

        public void StopAttacking()
        {
            CombatManager.OnWeaponTick -= FireOneShot;
        }

        public override void OnCombatExit()
        {
            base.OnCombatExit();
            if (combatant is AICombatant aic && !combatant.Health.IsDead)
            {
                aic.damageTracker.ClearTracker();
            }
        }

        public override Vector2 GetAimPosition()
        {
            if (currentTarget != null)
            {
                return currentTarget.Transform.position;
            }
            return base.GetAimPosition();
        }

        protected override void Death()
        {
            base.Death();

            //combatant.Health.Stat.statBar.gameObject.SetActive(false);
            Destroy(gameObject, 1.5f);
        }

        protected virtual void OnMouseEnter()
        {
            healthBarCanvas.OnMouseEnter();
        }

        protected virtual void OnMouseExit()
        {
            healthBarCanvas.OnMouseExit();
        }
    }
}