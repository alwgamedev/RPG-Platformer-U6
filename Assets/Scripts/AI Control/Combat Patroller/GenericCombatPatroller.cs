﻿using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
using UnityEngine;
using System;
using RPGPlatformer.Core;

namespace RPGPlatformer.AIControl
{
    public class GenericCombatPatroller<T0, T1> : GenericAIPatroller<T0>, ICombatPatroller
        where T0 : IAIMovementController
        where T1 : AICombatController
    {
        [SerializeField] protected float pursuitRange = 5;
        [SerializeField] protected float suspicionTime = 5;
        [SerializeField] protected bool playerEnemy = true;
        [SerializeField] protected bool setRunningWhenMaintainingMinimumCombatDistance;
        [SerializeField] protected Transform leftAttackBound;
        [SerializeField] protected Transform rightAttackBound;
        //[SerializeField] protected bool useHorizontalDistanceForCombatOnly;

        protected bool correctingCombatDistance;
        protected float suspicionTimer;
        protected IHealth currentTarget;
        protected T1 combatController;

        public virtual float TargetingTolerance => combatController.Combatant.Health.TargetingTolerance;
        public float MinimumCombatDistance => combatController.Combatant.IdealMinimumCombatDistance;
        public ICombatController CombatController => combatController;
        public IHealth CurrentTarget
        {
            get => currentTarget;
            protected set
            {
                currentTarget = value;
                combatController.currentTarget = value;
                MovementController.CurrentTarget = value?.transform;
            }
        }
        public Transform LeftAttackBound
        {
            get => leftAttackBound;
            set
            {
                leftAttackBound = value;
            }
        }
        public Transform RightAttackBound
        {
            get => rightAttackBound;
            set
            {
                rightAttackBound = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            combatController = GetComponent<T1>();
        }

        public override void InitializeState()
        {
            if (playerEnemy && GlobalGameTools.Instance)
            {
                CurrentTarget = GlobalGameTools.Instance.Player.Combatant.Health;
            }
            else
            {
                CurrentTarget = null;
            }

            if (CurrentTarget == null || !ScanForTarget(null))
            {
                Trigger(typeof(Patrol).Name);
            }
        }

        //public virtual void SetCombatTarget(IHealth targetHealth)
        //{
        //    CurrentTarget = targetHealth;

        //    //if (CurrentTarget != null)
        //    //{
        //    //    Trigger(typeof(Suspicion).Name);
        //    //}
        //    //else
        //    //{
        //    //    Trigger(typeof(Patrol).Name);
        //    //}
        //}

        protected bool ScanForTarget(Action TargetOutOfRange = null)
        {
            if (!TryGetDistanceSqrd(CurrentTarget, out float d2, out float t) 
                || !InRange(d2, pursuitRange, t))
            {
                TargetOutOfRange?.Invoke();
                return false;
            }

            if (CanAttackAtDistSqrd(d2, t))
            {
                Trigger(typeof(Attack).Name);
                return true;
            }

            Trigger(typeof(Pursuit).Name);
            return true;
        }

        protected virtual bool CanAttackAtDistSqrd(float d2, float t)
        {
            if (leftAttackBound && CurrentTarget.transform.position.x < leftAttackBound.transform.position.x)
            {
                return false;
            }

            if(rightAttackBound && CurrentTarget.transform.position.x > rightAttackBound.transform.position.x)
            {
                return false;
            }

            return combatController.Combatant.CanAttackAtDistSqrd(d2, t);
        }

        public override void PatrolBehavior()
        {
            if (CurrentTarget == null || !ScanForTarget(null))
            {
                base.PatrolBehavior();
            }
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

        public virtual void PursuitBehavior()
        {
            if (!TryGetDistanceSqrd(CurrentTarget, out float d2, out float t)
                || !InRange(d2, pursuitRange, t))
            {
                Trigger(typeof(Suspicion).Name);
                return;
            }
            else if (ShouldBreakPursuitToAttack(d2, t))
            {
                Trigger(typeof(Attack).Name);
            }
            else if (CanPursue(d2, t))
            //to avoid ai stuttering back and forth when their target is directly above/below them
            {
                Pursue(d2, t);
                //MovementController.MoveTowards(CurrentTarget.transform.position);
            }
            else
            {
                MovementController.SoftStop();
            }
        }

        protected virtual bool ShouldBreakPursuitToAttack(float distSquared, float tolerance)
        {
            return CanAttackAtDistSqrd(distSquared, tolerance);
        }

        //prevent ai from stuttering back and forth when target is directly overhead
        //(would want to change this for flying enemies)
        protected virtual bool CanPursue(float distSquared, float tolerance)
        {
            var d = CurrentTarget.transform.position.x - transform.position.x;
            return MovementController.CanMove(d * Vector3.right) 
                && !InRange(Mathf.Abs(d), MinimumCombatDistance, tolerance);
        }

        //allows child classes to decide how to pursue based on distance
        //(e.g. pill bug will switch to rolling if distance is far enough)
        protected virtual void Pursue(float distanceSquared, float tolerance)
        {
            MovementController.MoveTowards(CurrentTarget.transform.position);
        }

        public virtual void AttackBehavior()
        {
            if (!TryGetDistanceSqrd(CurrentTarget, out var d, out var t))
            {
                Trigger(typeof(Suspicion).Name);
            }
            else if (!combatController.Combatant.CanAttackAtDistSqrd(d, t))
            {
                OutOfRangeAttackBehavior(d, t);
            }
            else
            {
                InRangeAttackBehavior(d, t);
            }
        }

        public virtual void OutOfRangeAttackBehavior(float distanceSqrd, float tolerance)
        {
            Trigger(typeof(Pursuit).Name);
        }

        public virtual void InRangeAttackBehavior(float distanceSqrd, float tolerance)
        {
            MaintainMinimumCombatDistance(distanceSqrd, tolerance);
        }

        public void StartAttacking()
        {
            combatController.StartAttacking();

            if (!MovementController.Jumping)
            {
                MovementController.SoftStop();
            }
        }

        public void StopAttacking()
        {
            combatController.StopAttacking();
            correctingCombatDistance = false;
        }

        protected virtual void MaintainMinimumCombatDistance(float targetDistSqrd, float tolerance)
        {
            //not very performance-conscious, because he will continue scanning for drop offs
            //every frame that he is correcting combat distance,
            //but I think it's better to just be safe and reliable
            //(alternative is we tell the ai to just "go this direction, and keeping going without
            //reassessing until you're far enough away again"
            //-- in that case the player could theoretically walk the enemy over to a far away cliff (if the 
            //enemy doesn't reassess at any point while getting walked toward the cliff)
            if (InRange(targetDistSqrd, MinimumCombatDistance, tolerance))
            {
                float direction =
                    Mathf.Sign(transform.position.x - CurrentTarget.transform.position.x);
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
                if (setRunningWhenMaintainingMinimumCombatDistance)
                {
                    MovementController.SetRunning(true);
                }
                MovementController.MoveAwayFrom(currentTarget.transform.position);
            }
            else if (correctingCombatDistance)
            {
                correctingCombatDistance = false;
                MovementController.SetRunning(false);
                MovementController.SoftStop();
                combatController.FaceAimPosition();
            }
        }

        public bool InRange(float distSqrd, float range, float tolerance)
        {
            var a = range + tolerance;
            return distSqrd < a * a;
        }

        /// <summary>
        /// Tolerance output is your targetingTolerance + target's targetingTolerance.
        /// </summary>
        public bool TryGetDistanceSqrd(IHealth target, out float distanceSqrd, out float tolerance)
        {
            if (target != null && !target.IsDead)
            {
                distanceSqrd = Vector2.SqrMagnitude(transform.position - target.transform.position);
                tolerance = target.TargetingTolerance + TargetingTolerance;
                return true;
            }
            distanceSqrd = Mathf.Infinity;
            tolerance = Mathf.Infinity;
            return false;
        }
    }
}