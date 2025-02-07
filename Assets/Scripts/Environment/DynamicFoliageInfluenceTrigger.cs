using System.Collections;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class DynamicFoliageInfluenceTrigger : MonoBehaviour
    {
        [SerializeField] bool randomizeExternalInfluenceStrength;
        [Min(0)][SerializeField] float minExternalInfluenceStrength;
        [Min(0)][SerializeField] float maxExternalInfluenceStrength;

        DynamicFoliageController foliageController;
        Material foliageMaterial;

        bool easingIn;
        bool easingOut;

        float defaultInfluence;
        float externalInfluenceStrength = 1;

        int externalInfluenceProperty = Shader.PropertyToID("_ExternalInfluence");

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
            foliageMaterial = GetComponent<SpriteRenderer>().material;
            defaultInfluence = foliageMaterial.GetFloat(externalInfluenceProperty);
        }

        private bool CanTriggerInfluence(Collider2D collider)
        {
            return collider && collider.attachedRigidbody
                && Mathf.Abs(collider.attachedRigidbody.linearVelocityX) > foliageController.MinSpeedToTrigger;

        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!easingIn && CanTriggerInfluence(collider))
            {
                TryEaseIn(collider.attachedRigidbody.linearVelocityX);
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (!easingOut)
            {
                TryEaseOut();
            }
        }

        //Have to do this because TriggerEnter/Exit can get called even when game object is inactive,
        //and inactive game object can't start a coroutine (so without it we were getting a flood of errors every
        //time we exited play mode)
        private void TryEaseIn(float velocity)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(EaseIn(velocity));
            }
        }

        private void TryEaseOut()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(EaseOut());
            }
        }

        IEnumerator EaseIn(float velocity)
        {
            easingIn = true;
            easingOut = false;

            velocity *= externalInfluenceStrength;
            float timer = 0;

            while (timer < foliageController.EaseInTime)
            {
                if (!easingIn)
                {
                    yield break;
                }
                float progress = timer / foliageController.EaseInTime;
                float newVelocity = (1 - progress) * defaultInfluence + progress * velocity;
                foliageController.SetFoliageInfluence(foliageMaterial, newVelocity);
                timer += Time.deltaTime;
                yield return null;
            }

            if (easingIn)
            {
                foliageController.SetFoliageInfluence(foliageMaterial, velocity);
                easingIn = false;
            }
        }

        IEnumerator EaseOut()
        {
            easingOut = true;
            easingIn = false;

            float startingInfluence = foliageMaterial.GetFloat(externalInfluenceProperty);
            float timer = 0;

            while (timer < foliageController.EaseOutTime)
            {
                if (!easingOut)
                {
                    yield break;
                }
                float progress = timer / foliageController.EaseOutTime;
                float newVelocity = (1 - progress) * startingInfluence + progress * defaultInfluence;
                foliageController.SetFoliageInfluence(foliageMaterial, newVelocity);
                timer += Time.deltaTime;
                yield return null;
            }

            if (easingOut)
            {
                foliageController.SetFoliageInfluence(foliageMaterial, defaultInfluence);
                easingOut = false;
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}