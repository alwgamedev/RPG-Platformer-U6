using System;
using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class EarthwormMovementController : MonoBehaviour, ICombatantMovementController
    {
        [SerializeField] float moveSpeed = 0.5f;
        [SerializeField] float destinationTolerance = 0.1f;

        Vector3 currentDestination;
        Action OnUpdate;

        public bool Moving => false;

        public HorizontalOrientation CurrentOrientation => (HorizontalOrientation)Mathf.Sign(transform.localScale.x);

        public event Action DestinationReached;

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        public void GoTo(Vector3 point)
        {
            transform.position = point;
        }

        public void BeginMoveTowards(Vector3 destination)
        {
            currentDestination = destination;
            OnUpdate = MoveTowardsDestination;
        }

        public void MoveTowardsDestination()
        {
            var d = currentDestination - transform.position;
            var l = d.magnitude;

            if (l < destinationTolerance)
            {
                Stop();
                DestinationReached?.Invoke();
            }
            else
            {
                transform.position += moveSpeed * (d / l);
            }
        }

        public void Stop()
        {
            OnUpdate = null;
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
            OnUpdate = null;
            DestinationReached = null;
        }
    }
}