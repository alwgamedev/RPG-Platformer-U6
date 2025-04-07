using System;
using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class EarthwormMovementController : MonoBehaviour, ICombatantMovementController
    {
        //[SerializeField] float moveSpeed = 0.5f;
        [SerializeField] float destinationTolerance = 0.1f;

        Vector3 currentDestination;
        float currentSpeed;
        Action MoveAction;

        public bool Moving => false;

        public HorizontalOrientation CurrentOrientation => (HorizontalOrientation)Mathf.Sign(transform.localScale.x);

        public event Action DestinationReached;

        private void Update()
        {
            MoveAction?.Invoke();
        }

        public void GoTo(Vector3 point)
        {
            transform.position = point;
        }

        //public void BeginEmerge(Vector3 destination)
        //{
        //    currentDestination = destination;
        //    MoveAction = EmergeMoveAction;
        //}

        //public void BeginRetreat(Vector3 destination)
        //{
        //    currentDestination = destination;
        //    MoveAction = RetreatMoveAction;
        //}

        public void BeginMoveTowards(Vector3 destination, float moveSpeed)
        {
            currentDestination = destination;
            currentSpeed = moveSpeed;
            MoveAction = MoveTowardsDestination;
        }

        public void MoveTowardsDestination()
        {
            var d = currentDestination - transform.position;
            var l = Vector3.SqrMagnitude(d);

            if (l < destinationTolerance)
            {
                Stop();
                DestinationReached?.Invoke();
            }
            else
            {
                transform.position += currentSpeed * Time.deltaTime * (d / l);
            }
        }

        public void Stop()
        {
            MoveAction = null;
        }

        public void FaceTarget(Transform target)
        {
            FaceTarget(target.position);
        }

        public void FaceTarget(Vector3 target)
        {
            var d = target.x - transform.position.x;

            if (d != 0)
            {
                var s = transform.localScale;
                s.x = Mathf.Sign(d) * Mathf.Abs(d);
                transform.localScale = s;
            }
        }

        public void OnDeath()
        {
            Stop();   
        }

        public void OnRevival() { }

        private void OnDestroy()
        {
            MoveAction = null;
            DestinationReached = null;
        }
    }
}