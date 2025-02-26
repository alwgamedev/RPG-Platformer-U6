using System.Collections;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class DynamicFoliageInfluenceTrigger : MonoBehaviour
    {
        [SerializeField] bool randomizeExternalInfluenceStrength;
        [Min(0)][SerializeField] float minExternalInfluenceStrength;
        [Min(0)][SerializeField] float maxExternalInfluenceStrength;
        [SerializeField] float snapBackRate = 1;

        DynamicFoliageController foliageController;
        Material foliageMaterial;

        bool easingIn;
        bool easingOut;
        Collider2D influencingCollider;

        Vector2 defaultInfluence;
        float externalInfluenceStrength = 1;

        int influenceVelocityProperty = Shader.PropertyToID("_InfluenceVelocity");

        private void Awake()
        {
            if (randomizeExternalInfluenceStrength && minExternalInfluenceStrength <= maxExternalInfluenceStrength)
            {
                externalInfluenceStrength = Random.Range(minExternalInfluenceStrength, maxExternalInfluenceStrength);
            }
            else
            {
                externalInfluenceStrength = maxExternalInfluenceStrength;
            }
        }

        private void Start()
        {
            foliageController = GetComponent<DynamicFoliageController>();
            foliageMaterial = GetComponentInChildren<SpriteRenderer>().material;
            defaultInfluence = foliageMaterial.GetVector(influenceVelocityProperty);
        }

        private void Update()
        {
            if (influencingCollider != null && !influencingCollider.gameObject.activeInHierarchy)
            {
                EndInfluence(influencingCollider);
            }
        }

        private bool CanTriggerInfluence(Collider2D collider)
        {
            return collider && collider.attachedRigidbody
                && collider.attachedRigidbody.linearVelocity.magnitude > foliageController.MinSpeedToTrigger;

        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!easingIn && CanTriggerInfluence(collider))
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
            TryEaseIn(collider.attachedRigidbody.linearVelocity, orientation);
        }

        private void EndInfluence(Collider2D collider)
        {
            if (collider == influencingCollider)
            {
                influencingCollider = null;
                if (!easingOut)
                {
                    TryEaseOut();
                }
            }
        }



        //Have to do this because TriggerEnter/Exit can get called even when game object is inactive,
        //and inactive game object can't start a coroutine (so without it we were getting a flood of errors every
        //time we exited play mode)
        private void TryEaseIn(Vector2 velocity, float orientation)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(EaseIn(velocity, orientation));
            }
        }

        private void TryEaseOut()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(EaseOut());
            }
        }

        IEnumerator EaseIn(Vector2 velocity, float orientation)
        {
            easingIn = true;
            easingOut = false;
            foliageController.SetFoliageInfluenceOrientation(foliageMaterial, orientation);

            velocity *= externalInfluenceStrength;
            float timer = 0;

            while (timer < foliageController.EaseInTime)
            {
                if (!easingIn)
                {
                    yield break;
                }
                float progress = timer / foliageController.EaseInTime;
                Vector2 newVelocity = (1 - progress) * defaultInfluence + progress * velocity;
                foliageController.SetFoliageInfluenceVelocity(foliageMaterial, newVelocity);
                timer += Time.deltaTime;
                yield return null;
            }

            if (easingIn)
            {
                foliageController.SetFoliageInfluenceVelocity(foliageMaterial, velocity);
                easingIn = false;
            }
        }

        IEnumerator EaseOut()
        {
            easingOut = true;
            easingIn = false;

            Vector2 startingInfluence = foliageMaterial.GetVector(influenceVelocityProperty);
            float timer = 0;

            while (timer < foliageController.EaseOutTime)
            {
                if (!easingOut)
                {
                    yield break;
                }
                float p = timer / foliageController.EaseOutTime;
                p = Mathf.Pow(p, snapBackRate);
                Vector2 newVelocity = (1 - p) * startingInfluence 
                    + p * defaultInfluence;
                foliageController.SetFoliageInfluenceVelocity(foliageMaterial, newVelocity);
                timer += Time.deltaTime;
                yield return null;
            }

            if (easingOut)
            {
                foliageController.SetFoliageInfluenceVelocity(foliageMaterial, defaultInfluence);
                easingOut = false;
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}