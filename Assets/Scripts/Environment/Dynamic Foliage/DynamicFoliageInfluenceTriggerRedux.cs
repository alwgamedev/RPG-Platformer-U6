using RPGPlatformer.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class DynamicFoliageInfluenceTriggerRedux : MonoBehaviour
    {
        [SerializeField] bool randomizeExternalInfluenceStrength;
        [Min(0)][SerializeField] float minExternalInfluenceStrength;
        [Min(0)][SerializeField] float maxExternalInfluenceStrength;

        DynamicFoliageControllerRedux foliageController;

        float influenceStrength = 1;
        Collider2D influencingCollider;

        private void Awake()
        {
            if (randomizeExternalInfluenceStrength && minExternalInfluenceStrength <= maxExternalInfluenceStrength)
            {
                influenceStrength
                    = UnityEngine.Random.Range(minExternalInfluenceStrength, maxExternalInfluenceStrength);
            }
            else
            {
                influenceStrength = maxExternalInfluenceStrength;
            }
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
            if (CanTriggerInfluence(collider))
            {
                BeginInfluence(collider);
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            EndInfluence(collider);
        }

        private void BeginInfluence(Collider2D collider)
        {
            influencingCollider = collider;

            float orientation = Mathf.Sign(collider.transform.localScale.x);
            if ((transform.position.x < collider.transform.position.x && orientation > 0)
                || (transform.position.x > collider.transform.position.x && orientation < 0))
            {
                orientation *= -1;
            }
            BeginEaseIn(collider.attachedRigidbody.linearVelocity, orientation);
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
            if (gameObject.activeInHierarchy)
            {
                foliageController.BeginEaseIn(velocity * influenceStrength, orientation);
            }
        }

        private void BeginEaseOut()
        {
            if (gameObject.activeInHierarchy)
            {
                foliageController.BeginEaseOut();
            }
        }
    }
}