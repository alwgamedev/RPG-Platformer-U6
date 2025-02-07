using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class DynamicFoliageInfluenceTrigger : MonoBehaviour
    {
        [SerializeField] float externalInfluenceStrength = 1;

        DynamicFoliageController foliageController;
        Material foliageMaterial;

        //GameObject player;
        //Rigidbody2D playerRb;

        bool easingIn;
        //bool easingOut;

        float defaultInfluence;
        float velocityLastFrame;

        int externalInfluenceProperty = Shader.PropertyToID("_ExternalInfluence");

        private void Start()
        {
            //player = GameObject.FindWithTag("Player");
            //playerRb = player.GetComponent<Rigidbody2D>();

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
                StartCoroutine(EaseIn(collider.attachedRigidbody.linearVelocityX));
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            StartCoroutine(EaseOut());
            //slightly worried this will get triggered by ground, but we'll see
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (!easingIn && CanTriggerInfluence(collider))
            {
                OnTriggerEnter2D(collider);
            }

            else if (easingIn &&  !CanTriggerInfluence(collider))
            {
                StartCoroutine(EaseOut());
            }

        }

        IEnumerator EaseIn(float xVelocity)
        {
            easingIn = true;
            xVelocity *= externalInfluenceStrength;
            float timer = 0;

            while (timer < foliageController.EaseInTime)
            {
                float progress = timer / foliageController.EaseInTime;
                float newVelocity = (1 - progress) * defaultInfluence + progress * xVelocity;
                foliageController.SetFoliageInfluence(foliageMaterial, newVelocity);
                timer += Time.deltaTime;
                yield return null;
            }

            foliageController.SetFoliageInfluence(foliageMaterial, xVelocity);
            easingIn = false;
        }

        IEnumerator EaseOut()
        {
            //easingOut = true;
            float startingInfluence = foliageMaterial.GetFloat(externalInfluenceProperty);
            float timer = 0;

            while (timer < foliageController.EaseOutTime)
            {
                float progress = timer / foliageController.EaseOutTime;
                float newVelocity = (1 - progress) * startingInfluence + progress * defaultInfluence;
                foliageController.SetFoliageInfluence(foliageMaterial, newVelocity);
                timer += Time.deltaTime;
                yield return null;
            }

            foliageController.SetFoliageInfluence(foliageMaterial, defaultInfluence);
            //easingOut = false;
        }
    }
}