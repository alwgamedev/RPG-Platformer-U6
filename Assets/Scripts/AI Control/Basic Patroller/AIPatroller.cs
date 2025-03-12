using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
using System;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AIMovementController))]
    //[RequireComponent(typeof(AICombatController))]
    public class AIPatroller : StateDriver
    {
        [SerializeField] protected float pursuitRange;
        [SerializeField] protected float suspicionTime = 5;
        [SerializeField] protected float hangTime = 2;//e.g. to stop for a few seconds btwn patrol destinations

        protected bool TriggerPursuitSubscribedToCombatantTargetingFailed;
        protected float suspicionTimer;
        protected float hangTimer;
        protected Vector2 patrolDestination;
        protected Action OnUpdate;

        public AIMovementController MovementController { get; protected set; }
        public AICombatController CombatController { get; protected set; }

        public IHealth CombatTarget
        {
            get => CombatController != null ? CombatController.currentTarget : null;
            protected set
            {
                if (CombatController != null)
                {
                    CombatController.currentTarget = value;
                    MovementController.currentTarget = value;
                }
            }
        }

        protected virtual void Awake()
        {
            MovementController = GetComponent<AIMovementController>();
            CombatController = GetComponent<AICombatController>();
        }

        protected virtual void Start()
        {
            if (CombatController == null)
            {
                return;
            }
            if (CombatController.CombatManager != null)
            {
                OnCombatManagerConfigured();
            }
            else
            {
                CombatController.CombatManagerConfigured += OnCombatManagerConfigured;
            }
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        protected void OnCombatManagerConfigured()
        {
            CombatController.CombatManager.OnWeaponTick += CheckMinimumCombatDistance;
            CombatController.CombatManagerConfigured -= OnCombatManagerConfigured;
        }

        public virtual void SetCombatTarget(IHealth targetHealth)
        {
            CombatTarget = targetHealth;

            if (CombatTarget != null)
            {
                TriggerSuspicion();
            }
            else
            {
                Trigger(typeof(Patrol).Name);
            }
        }

        protected bool ScanForTarget(Action TargetOutOfRange = null)
        {
            if (!TryGetDistance(CombatTarget, out float distance) || distance > pursuitRange)
            {
                TargetOutOfRange?.Invoke();
                return false;
            }

            if (CombatController != null && CombatController.Combatant.CanAttack(distance))
            {
                Trigger(typeof(Attack).Name);
                return true;
            }

            Trigger(typeof(Pursuit).Name);
            return true;
        }

        public virtual void BeginPatrol() { }

        public virtual void PatrolBehavior() 
        {
            ScanForTarget(null);
        }

        //public void DefaultPatrolBehavior()
        //{
        //    if (CombatTarget != null && ScanForTarget(null))
        //    {
        //        return;
        //    }
        //    else if (PatrolDestinationReached(patrolDestination))
        //    {
        //        OnPatrolDestinationReached();
        //    }
        //    else
        //    {
        //        MovementController.MoveTowards(patrolDestination);
        //    }
        //}

        ////useful to have this as a virtual method -- most of the time we will just check
        ////distance < tolerance, but some patrollers may only care about x-value (e.g. if bounded patroller
        ////chose a random destination, the y-value of the point may be below ground, making it impossible to get
        ////within tolerance of the point, so the bounded patroller should only check x-value)
        //protected virtual bool PatrolDestinationReached(Vector2 destination)
        //{
        //    return false;
        //}

        //protected virtual void OnPatrolDestinationReached() 
        //{
        //    //e.g. calculate next patrol destination
        //}

        public virtual void SuspicionBehavior()
        {
            ScanForTarget(TimedSuspicion);
        }

        protected virtual void TimedSuspicion()
        {
            suspicionTimer += Time.deltaTime;

            if (suspicionTimer > suspicionTime)
            {
                Trigger(typeof(Patrol).Name);
            }
        }

        public void ResetSuspicionTimer()
        {
            suspicionTimer = 0;
        }

        public void PursuitBehavior()
        {
            if (!TryGetDistance(CombatTarget, out float distance) || distance > pursuitRange)
            {
                Trigger(typeof(Suspicion).Name);
            }
            else if (CombatController != null && CombatController.Combatant.CanAttack(distance))
            {
                Trigger(typeof(Attack).Name);
            }
            //else if (distance > pursuitRange)
            //{
            //    Trigger(typeof(Suspicion).Name);
            //}
            else if (Mathf.Abs(CombatTarget.Transform.position.x - transform.position.x) > 0.25f)
                //to avoid ai stuttering back and forth when their target is directly above them
            {
                MovementController.MoveTowards(CombatTarget.Transform.position);
            }
            else
            {
                MovementController.MoveInput = 0;
            }
        }

        public void StartAttacking()
        {
            if (CombatController == null) return;
            SubscribeTriggerPursuitToCombatantTargetingFailed(true);
            CombatController.StartAttacking();

        }

        public void StopAttacking()
        {
            if (CombatController == null) return;
            SubscribeTriggerPursuitToCombatantTargetingFailed(false);
            CombatController.StopAttacking();
        }

        protected void SubscribeTriggerPursuitToCombatantTargetingFailed(bool val)
        {
            if (val == TriggerPursuitSubscribedToCombatantTargetingFailed) return;

            if (val)
            {
                CombatController.Combatant.OnTargetingFailed += TriggerPursuit;
                TriggerPursuitSubscribedToCombatantTargetingFailed = true;
            }
            else
            {
                CombatController.Combatant.OnTargetingFailed -= TriggerPursuit;
                TriggerPursuitSubscribedToCombatantTargetingFailed = false;
            }
        }

        public void CheckMinimumCombatDistance()
        {
            OnUpdate -= MaintainMinimumCombatDistance;

            if (!TryGetDistance(CombatTarget, out var d))
            {
                return;
            }

            if (d < CombatController.AICombatant.MinimumCombatDistance)
            {
                OnUpdate += MaintainMinimumCombatDistance;
            }
        }

        public void MaintainMinimumCombatDistance()
        {
            if (!TryGetDistance(CombatTarget, out var d))
            {
                OnUpdate -= MaintainMinimumCombatDistance;
                return;
            }
            else if (d < CombatController.AICombatant.MinimumCombatDistance)
            {
                MovementController.MoveAwayFrom(CombatTarget.Transform.position);
            }
            else
            {
                MovementController.MoveInput = 0;
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

        public void TriggerPursuit()
        {
            Trigger(typeof(Pursuit).Name);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnUpdate = null;
        }
    }
}