using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class BoundedPatroller : AIPatroller
    {
        [SerializeField] protected Transform leftBound;
        [SerializeField] protected Transform rightBound;
        [SerializeField] protected float patrolDestinationTolerance = 0.1f;

        public override void PatrolBehavior()
        {
            DefaultPatrolBehavior();
        }

        protected override bool PatrolDestinationReached(Vector2 destination)
        {
            return Mathf.Abs(transform.position.x - patrolDestination.x) < patrolDestinationTolerance;
        }

        protected override void OnPatrolDestinationReached()
        {
            patrolDestination = Random.Range(leftBound.position.x, rightBound.position.x) * Vector2.right;
        }

        //protected override void SuspiciousButTargetOutOfRange()
        //{
        //    suspicionTimer += Time.deltaTime;

        //    if (suspicionTimer > suspicionTimerMax)
        //    {
        //        TriggerPatrol();
        //    }
        //}

        //public override void EndSuspicion()
        //{
        //    suspicionTimer = 0;
        //}
    }
}