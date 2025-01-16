using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Combat;
using RPGPlatformer.Movement;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AdvancedMovementController))]
    [RequireComponent(typeof(AICombatController))]
    public class AIPatroller : StateDriver
    {
        [SerializeField] protected float pursuitRange;

        public AdvancedMovementController movementController;
        public AICombatController combatController;


        protected virtual void Awake()
        {
            movementController = GetComponent<AdvancedMovementController>();
            combatController = GetComponent<AICombatController>();
        }

        protected virtual void Start()
        {
            //combatController.OnWeaponTick += combatController.FireOneShot;
        }

        public void PatrolBehavior()
        {
            //stand there for now
        }

        public void SuspicionBehavior(IHealth target)
        {
            if (!TryGetDistance(target, out float distance))
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

        public void PursuitBehavior(IHealth target)
        {
            if (!TryGetDistance(target, out float distance))
            {
                Trigger(typeof(Suspicion).Name);
                return;
            }
            if (combatController.Combatant.CanAttack(distance))
            {
                Trigger(typeof(Attack).Name);
            }
            else if (distance > pursuitRange)
            {
                Trigger(typeof(Suspicion).Name);
            }
            else
            {
                movementController.MoveInput = target.Transform.position.x - transform.position.x;
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

        public bool TryGetDistance(IHealth target, out float distance)
        {
            if (target != null && !target.IsDead)
            {
                distance = Vector3.Distance(transform.position, target.Transform.position);
                return true;
            }
            else
            {
                distance = Mathf.Infinity;
                return false;
            }
        }

        public void BeginPatrol()
        {
            Trigger(typeof(Patrol).Name);
        }

        public void BecomeSuspicious()
        {
            Trigger(typeof(Suspicion).Name);
        }
    }
}