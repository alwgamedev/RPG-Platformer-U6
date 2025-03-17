using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class PatrolPoint : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collider)
        {
            HandleTrigger(collider);
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            HandleTrigger(collider);
            //doing both enter and stay (despite performance cost),
            //because a patroller may want to begin the path while already inside the collider of the first point,
            //and if we only had triggerenter then he would never know to go to the next point
        }

        private void HandleTrigger(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy) return;//because trigger still goes off when inactive

            if (collider.gameObject.TryGetComponent(out IPathPatroller p)
                && p.TargetPoint?.Value == this)
            {
                p.OnDestinationReached();
            }
        }
    }
}