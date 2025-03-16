using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
using System;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AIMovementController))]
    [RequireComponent(typeof(PatrolNavigator))]
    //[RequireComponent(typeof(AICombatController))]
    public class AIPatroller : StateDriver
    {
        [SerializeField] protected float pursuitRange = 5;
        [SerializeField] protected float suspicionTime = 5;
        protected bool correctingCombatDistance;
        protected float suspicionTimer;
        protected float hangTimer;
        protected IHealth currentTarget;

        protected Action OnUpdate;

        public float TargetingTolerance => CombatController.Combatant.Health.TargetingTolerance;
        public float MinimumCombatDistance => CombatController.AICombatant.MinimumCombatDistance;
        public PatrolNavigator PatrolNavigator { get; protected set; }
        public AIMovementController MovementController { get; protected set; }
        public AICombatController CombatController { get; protected set; }

        public IHealth CurrentTarget
        {
            get => currentTarget;
            protected set
            {
                currentTarget = value;
                if (CombatController != null)
                {
                    CombatController.currentTarget = value;
                }
                if (MovementController != null)
                {
                    MovementController.currentTarget = value;
                }
            }
        }

        protected virtual void Awake()
        {
            MovementController = GetComponent<AIMovementController>();
            CombatController = GetComponent<AICombatController>();
            PatrolNavigator = GetComponent<PatrolNavigator>();

            PatrolNavigator.PatrolComplete += OnPatrolComplete;
            PatrolNavigator.BeginHangTime += MovementController.SoftStop;
        }

        protected virtual void Start()
        {
            if (CombatController == null)
            {
                return;
            }

            //TargetingTolerance = CombatController.Combatant.Health.TargetingTolerance;

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
            //CombatController.CombatManager.OnWeaponTick += CheckMinimumCombatDistance;
            CombatController.CombatManagerConfigured -= OnCombatManagerConfigured;
        }

        public virtual void SetCombatTarget(IHealth targetHealth)
        {
            CurrentTarget = targetHealth;

            if (CurrentTarget != null)
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
            if (!TryGetDistance(CurrentTarget, out float distance) || distance > pursuitRange)
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

        public virtual void BeginPatrol(PatrolMode mode, PatrolParemeters p)
        {
            PatrolNavigator.BeginPatrol(mode, p);
        }

        public virtual void PatrolBehavior()
        {
            if (CurrentTarget == null || !ScanForTarget(null))
            {
                PatrolNavigator.PatrolBehavior(MovementController);
            }
        }

        //this gets called e.g. when you reach the end of a patrol path
        public virtual void OnPatrolComplete() 
        {
            MovementController.SoftStop();
            PatrolNavigator.BeginRest();
        }


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
            if (!TryGetDistance(CurrentTarget, out float distance) || distance > pursuitRange)
            {
                Trigger(typeof(Suspicion).Name);
                return;
            }
            else if (CombatController != null && CombatController.Combatant.CanAttack(distance))
            {
                Trigger(typeof(Attack).Name);
            }
            else if (Mathf.Abs(CurrentTarget.Transform.position.x - transform.position.x) > MinimumCombatDistance)
                //to avoid ai stuttering back and forth when their target is directly above them
            {
                MovementController.MoveTowards(CurrentTarget.Transform.position);
            }
            else
            {
                MovementController.SoftStop();
            }
        }

        public void AttackBehavior()
        {
            if (!TryGetDistance(CurrentTarget, out var d))
            {
                TriggerSuspicion();
            }
            else if (!CombatController.Combatant.CanAttack(d))
            {
                TriggerPursuit();
            }
            else
            {
                MaintainMinimumCombatDistance(d);
            }
        }

        public void StartAttacking()
        {
            if (CombatController == null) return;
            CombatController.StartAttacking();
            MovementController.SoftStop();

        }

        public void StopAttacking()
        {
            if (CombatController == null) return;
            CombatController.StopAttacking();
            correctingCombatDistance = false;
        }

        public void MaintainMinimumCombatDistance(float currentDistance)
        {
            //not very performance-conscious, because he will continue scanning for drop offs
            //every frame that he is correcting combat distance,
            //but I think it's better to just be safe and reliable
            //(alternative is we tell the ai to just "go this direction, and keeping going without
            //reassessing until you're far enough away again"
            //-- in that case the player could theoretically walk the enemy over to a far away cliff (if the 
            //enemy doesn't reassess at any point while getting walked toward the cliff)
            if (currentDistance < CombatController.AICombatant.MinimumCombatDistance)
            {
                float direction =
                    Mathf.Sign(transform.position.x - CurrentTarget.Transform.position.x);
                if (MovementController.DropOffAhead((HorizontalOrientation)direction, out var d) 
                    && d < 1.5f * MinimumCombatDistance)
                {
                    direction = -direction;

                    if (MovementController.DropOffAhead((HorizontalOrientation)direction, out d)
                        && d < 1.5f * MinimumCombatDistance)
                    {
                        return;
                    }
                }

                correctingCombatDistance = true;
                MovementController.MoveInput = new(direction, 0);
            }
            else if (correctingCombatDistance)
            {
                correctingCombatDistance = false;
                MovementController.SoftStop();
            }
        }

        public bool TryGetDistance(IHealth target, out float distance)
        {
            if (target != null && !target.IsDead)
            {
                distance = Vector3.Distance(transform.position, target.Transform.position) 
                    - target.TargetingTolerance - TargetingTolerance;
                //Debug.Log("patroller calculated distance: " + distance);
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