using RPGPlatformer.UI;
using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    public class AICombatController : CombatController
    {
        protected CombatantHealthBarCanvas healthBarCanvas;

        public IHealth currentTarget;

        public CombatStateManager CombatManager => combatManager;
        public AICombatant AICombatant { get; protected set; }

        protected override void Awake()
        {
            base.Awake();

            AICombatant = (AICombatant)combatant;
        }

        protected override void Start()
        {
            healthBarCanvas = GetComponentInChildren<CombatantHealthBarCanvas>();
            if (healthBarCanvas != null)
            {
                healthBarCanvas.Configure(this);
            }

            base.Start();
        }

        public void FireOneShot()
        {
            FaceAimPosition();
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