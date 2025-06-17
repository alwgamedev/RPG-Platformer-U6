using System;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    [Serializable]
    public class PatrolPathWayPoint : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy) return;

            HandleTrigger(collider);
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy) return;

            HandleTrigger(collider);
            //doing both enter and stay
            //because a patroller may begin the path while already inside the collider of the first point
        }

        private void HandleTrigger(Collider2D collider)
        {
            if (collider.gameObject.TryGetComponent(out IPathPatroller p)
                && p.TargetPoint?.Value == this)
            {
                p.OnDestinationReached();
            }
        }
    }
}