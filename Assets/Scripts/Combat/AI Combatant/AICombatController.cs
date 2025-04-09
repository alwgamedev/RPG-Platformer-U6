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
        [SerializeField] bool autoRetaliate = true;

        protected CombatantHealthBarCanvas healthBarCanvas;
        protected bool attacking;

        public IHealth currentTarget;

        protected override void Start()
        {
            healthBarCanvas = GetComponentInChildren<CombatantHealthBarCanvas>();
            if (healthBarCanvas != null)
            {
                healthBarCanvas.Configure(this);
            }

            base.Start();
        }

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();
        }

        public void FireOneShot()
        {
            FaceAimPosition();
            RunAutoAbilityCycle(false);
        }

        public void StartAttacking()
        {
            if (attacking) return;

            FireOneShot();
            stateManager.OnWeaponTick += FireOneShot;
            attacking = true;
        }

        public void StopAttacking()
        {
            if (!attacking) return;

            stateManager.OnWeaponTick -= FireOneShot;
            if (ChannelingAbility)
            {
                CancelAbilityInProgress(false);
            }

            attacking = false;
        }

        public override void OnCombatEntry()
        {
            base.OnCombatEntry();

            if (autoRetaliate)
            {
                StartAttacking();
                //start attacking if not already
            }
        }

        public override void OnCombatExit()
        {
            base.OnCombatExit();

            StopAttacking();
            //a) so that we get the immediate FireOneShot next time you enter combat (if auto-retaliate is on)
            //b) more importantly so we don't have dangling subscribers to OnWeaponTick that could accidentally
                //get doubly subscribed next time we enter combat

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

        protected virtual void OnMouseEnter()
        {
            if (healthBarCanvas)
            {
                healthBarCanvas.OnMouseEnter();
            }
        }

        protected virtual void OnMouseExit()
        {
            if (healthBarCanvas)
            {
                healthBarCanvas.OnMouseExit();
            }
        }
    }
}