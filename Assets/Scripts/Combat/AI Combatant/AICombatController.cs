using RPGPlatformer.Core;
using RPGPlatformer.UI;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    using T1 = CombatStateGraph;
    using T2 = CombatStateMachine;
    using T3 = AICombatant;
    using T4 = AnimationControl;

    public class AICombatController : GenericCombatController<CombatStateManager<T1, T2, T3, T4>, T1, T2, T3, T4>
    {
        [SerializeField] protected CombatantHealthBarCanvas healthBarCanvas;

        public bool autoRetaliate = true;

        //protected CombatantHealthBarCanvas healthBarCanvas;

        public IHealth currentTarget;

        public bool Attacking { get; protected set; }

        protected override void Start()
        {
            if (!healthBarCanvas)
            {
                healthBarCanvas = GetComponentInChildren<CombatantHealthBarCanvas>();
            }
            if (healthBarCanvas)
            {
                healthBarCanvas.Configure(this);
            }

            base.Start();
        }

        //NOTE: it returns the drop items in the order they are listed in the table...
        //so if table is bigger than inventory, you will never see items at the end of the table...
        //this may be something to work on
        //(I was originally going to choose random entries from the table for each inventory slot,
        //but if you "randomly" choose all rare drop table entries you could end up with no loot.
        //Maybe should use a more complicated system where rare items have a fallback common item
        //they can give if their drop fails)
        protected override void InitializeInventoryItems()
        {
            if (stateDriver.DropTable != null)
            {
                stateDriver.TakeLoot(stateDriver.DropTable.GenerateDrop(stateDriver.DropSize), false);
                //note that drop items beyond the inventory size will be ignored
                //(so set inventory size accordingly)
            }
        }

        public void AutoAttack()
        {
            FaceAimPosition();
            RunAutoAbilityCycle(false);
        }

        public void StartAttacking()
        {
            if (Attacking) return;

            Attacking = true;
            AutoAttack();
            stateManager.OnWeaponTick += AutoAttack;
        }

        public void StopAttacking()
        {
            stateManager.OnWeaponTick -= AutoAttack;
            if (ChannelingAbility)
            {
                CancelAbilityInProgress();
            }

            Attacking = false;
        }

        public override void MaximumPowerAchieved()
        {
            base.MaximumPowerAchieved();

            FireButtonUp();
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
                stateDriver.DamageTracker.ClearTracker();
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

        public override void OnInputDisabled()
        {
            StopAttacking();

            base.OnInputDisabled();
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