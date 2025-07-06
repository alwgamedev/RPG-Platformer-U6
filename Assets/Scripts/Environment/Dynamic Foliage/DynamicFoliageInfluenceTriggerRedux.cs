using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class DynamicFoliageInfluenceTriggerRedux : MonoBehaviour
    {
        [SerializeField] RandomizableFloat influenceStrength;

        float _influenceStrength;
        DynamicFoliageControllerRedux foliageController;

        Collider2D influencingCollider;

        private void Awake()
        {
            _influenceStrength = influenceStrength.Value;
        }

        private void Start()
        {
            foliageController = GetComponent<DynamicFoliageControllerRedux>();
        }

        private void Update()
        {
            if (influencingCollider != null && !influencingCollider.gameObject.activeInHierarchy)
            {
                EndCurrentInfluence();
            }
        }

        private bool CanTriggerInfluence(Collider2D collider)
        {
            return collider && collider.attachedRigidbody;

        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (gameObject.activeInHierarchy && CanTriggerInfluence(collider))
            {
                BeginInfluence(collider);
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (gameObject.activeInHierarchy)
            {
                EndInfluence(collider);
            }
        }

        private void BeginInfluence(Collider2D collider)
        {
            influencingCollider = collider;
            BeginEaseIn(collider.attachedRigidbody.linearVelocity, Orientation(collider.transform));
        }

        private float Orientation(Transform t)
        {
            float orientation = Mathf.Sign(t.localScale.x);
            if ((transform.position.x < t.position.x && orientation > 0)
                || (transform.position.x > t.position.x && orientation < 0))
            {
                orientation *= -1;
            }
            return orientation * Mathf.Sign(transform.lossyScale.x);
        }

        private void EndInfluence(Collider2D collider)
        {
            if (collider == influencingCollider)
            {
                influencingCollider = null;
                BeginEaseOut();
            }
        }

        private void EndCurrentInfluence()
        {
            if (influencingCollider != null)
            {
               EndInfluence(influencingCollider);
            }
        }

        private void BeginEaseIn(Vector2 velocity, float orientation)
        {
            foliageController.BeginEaseIn(velocity * _influenceStrength, orientation);
        }

        private void BeginEaseOut()
        {
            foliageController.BeginEaseOut();
        }
    }
}