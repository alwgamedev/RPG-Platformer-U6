using RPGPlatformer.Core;
using RPGPlatformer.UI;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    using T1 = CombatStateGraph;
    using T2 = CombatStateMachine;
    using T3 = AICombatant;
    using T4 = AnimationControl;

    public class AICombatController : GenericCombatController<CombatStateManager<T1, T2, T3, T4>, T1, T2, T3, T4>
    {
        protected CombatantHealthBarCanvas healthBarCanvas;

        public IHealth currentTarget;

        //public CombatStateManager CombatManager => combatManager;
        //public AICombatant AICombatant { get; protected set; }

        protected override void Awake()
        {
            base.Awake();

            //should make a generic combat controller so we don't have to do this
            //AICombatant = (AICombatant)stateDriver;
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
            stateManager.OnWeaponTick += FireOneShot;
            stateDriver.Attack();
        }

        public void StopAttacking()
        {
            stateManager.OnWeaponTick -= FireOneShot;
        }

        public override void OnCombatExit()
        {
            base.OnCombatExit();

            if (!stateDriver.Health.IsDead)
            {
                stateDriver.damageTracker.ClearTracker();
            }
        }

        public override Vector2 GetAimPosition()
        {
            if (currentTarget != null)
            {
                return currentTarget.transform.position;
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