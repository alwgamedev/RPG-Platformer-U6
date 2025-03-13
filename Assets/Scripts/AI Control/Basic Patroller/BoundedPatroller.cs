using System;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class BoundedPatroller : AIPatroller
    {
        [SerializeField] protected Transform leftBound;
        [SerializeField] protected Transform rightBound;
        [SerializeField] protected float patrolDestinationTolerance = 0.1f;

        protected Action PatrolAction;

        public override void BeginPatrol()
        {
            patrolDestination = NewPatrolDestination();
            PatrolAction = MoveTowardsPatrolDestination;
        }

        public override void PatrolBehavior()
        {
            PatrolAction?.Invoke();
        }

        public virtual void MoveTowardsPatrolDestination()
        {
            if (CurrentTarget != null && ScanForTarget(null))
            {
                return;
            }
            if (PatrolDestinationReached(patrolDestination))
            {
                if (hangTime > 0)
                {
                    MovementController.HardStop();
                    hangTimer = 0;
                    PatrolAction = HangOut;
                }
                else
                {
                    BeginPatrol();
                }
            }
            else
            {
                MovementController.MoveTowards(patrolDestination);
            }
        }

        public virtual void HangOut()
        {
            if (CurrentTarget != null && ScanForTarget(null))
            {
                return;
            }

            hangTimer += Time.deltaTime;

            if (hangTimer > hangTime)
            {
                BeginPatrol();
            }
        }

        protected virtual bool PatrolDestinationReached(Vector2 destination)
        {
            return Mathf.Abs(transform.position.x - patrolDestination.x) < patrolDestinationTolerance;
        }

        protected Vector2 NewPatrolDestination()
        {
            return UnityEngine.Random.Range(leftBound.position.x, rightBound.position.x) * Vector2.right;
            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            PatrolAction = null;
        }
    }
}