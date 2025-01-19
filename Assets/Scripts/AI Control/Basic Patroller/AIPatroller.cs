using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
using System;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AdvancedMovementController))]
    [RequireComponent(typeof(AICombatController))]
    public class AIPatroller : StateDriver
    {
        [SerializeField] protected float pursuitRange;

        protected Action OnUpdate;

        public AdvancedMovementController movementController;
        public AICombatController combatController;

        public IHealth CurrentTarget => combatController.currentTarget;


        protected virtual void Awake()
        {
            movementController = GetComponent<AdvancedMovementController>();
            combatController = GetComponent<AICombatController>();
        }

        protected virtual void OnEnable()
        {
            combatController.CombatManager.OnWeaponTick += CheckMinimumCombatDistance;
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        public void PatrolBehavior()
        {
            //stand there for now
        }

        public void SuspicionBehavior(/*IHealth target*/)
        {
            if (!TryGetDistance(CurrentTarget, out float distance))
            {
                return;
            }
            if (combatController.Combatant.CanAttack(distance))
            {
                Trigger(typeof(Attack).Name);
            }
            else if (distance < pursuitRange)
            {
                Trigger(typeof(Pursuit).Name);
            }
        }

        public void PursuitBehavior(/*IHealth target*/)
        {
            if (!TryGetDistance(CurrentTarget, out float distance))
            {
                Trigger(typeof(Suspicion).Name);
            }
            else if (combatController.Combatant.CanAttack(distance))
            {
                Trigger(typeof(Attack).Name);
            }
            else if (distance > pursuitRange)
            {
                Trigger(typeof(Suspicion).Name);
            }
            else
            {
                movementController.MoveTowards(CurrentTarget.Transform.position);
            }
        }

        public void StartAttacking()
        {
            combatController.Combatant.OnTargetingFailed += () => Trigger(typeof(Pursuit).Name);
            combatController.StartAttacking();

        }

        public void StopAttacking()
        {
            combatController.Combatant.OnTargetingFailed -= () => Trigger(typeof(Pursuit).Name);
            combatController.StopAttacking();
        }

        public void CheckMinimumCombatDistance()
        {
            OnUpdate -= MaintainMinimumCombatDistance;

            if (!TryGetDistance(CurrentTarget, out var d))
            {
                return;
            }

            if (d < combatController.MinimumCombatDistance)
            {
                OnUpdate += MaintainMinimumCombatDistance;
            }
        }

        public void MaintainMinimumCombatDistance()
        {
            if (!TryGetDistance(CurrentTarget, out var d))
            {
                OnUpdate -= MaintainMinimumCombatDistance;
                return;
            }
            else if (d < combatController.MinimumCombatDistance)
            {
                movementController.MoveAwayFrom(CurrentTarget.Transform.position);
            }
            else
            {
                movementController.MoveInput = 0;
                OnUpdate -= MaintainMinimumCombatDistance;
            }
        }

        public bool TryGetDistance(IHealth target, out float distance)
        {
            if (target != null && !target.IsDead)
            {
                distance = Vector3.Distance(transform.position, target.Transform.position);
                return true;
            }
            distance = Mathf.Infinity;
            return false;
        }

        public void TriggerPatrol()
        {
            Trigger(typeof(Patrol).Name);
        }

        public void TriggerSuspicion()
        {
            Trigger(typeof(Suspicion).Name);
        }
    }
}