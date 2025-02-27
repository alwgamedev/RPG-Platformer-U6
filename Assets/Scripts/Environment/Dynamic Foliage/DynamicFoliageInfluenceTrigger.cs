using RPGPlatformer.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class DynamicFoliageInfluenceTrigger : MonoBehaviour
    {
        [SerializeField] bool randomizeExternalInfluenceStrength;
        [Min(0)][SerializeField] float minExternalInfluenceStrength;
        [Min(0)][SerializeField] float maxExternalInfluenceStrength;
        [SerializeField] float snapBackRate = 1;

        CancellationTokenSource lifeCTS;

        DynamicFoliageController foliageController;
        Material foliageMaterial;

        bool easingIn;
        bool easingOut;
        Collider2D influencingCollider;
        Action OnInfluencingColliderDisabled;

        Vector2 defaultInfluence;
        float externalInfluenceStrength = 1;

        int influenceVelocityProperty = Shader.PropertyToID("_InfluenceVelocity");

        private void Awake()
        {
            if (randomizeExternalInfluenceStrength && minExternalInfluenceStrength <= maxExternalInfluenceStrength)
            {
                externalInfluenceStrength
                    = UnityEngine.Random.Range(minExternalInfluenceStrength, maxExternalInfluenceStrength);
            }
            else
            {
                externalInfluenceStrength = maxExternalInfluenceStrength;
            }
        }

        private void OnEnable()
        {
            InitializeCTSAsap();
            OnInfluencingColliderDisabled = async () => await EndCurrentInfluence();
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
                OnInfluencingColliderDisabled?.Invoke();
                //EndInfluence(influencingCollider);
            }
        }

        private void InitializeCTSAsap()
        {
            if (GlobalGameTools.Instance)
            {
                InitializeCTS();
            }
            else
            {
                GlobalGameTools.InstanceReady += InitializeCTS;
            }
        }

        private void InitializeCTS()
        {
            lifeCTS = CancellationTokenSource.CreateLinkedTokenSource(GlobalGameTools.Instance.TokenSource.Token);
            GlobalGameTools.InstanceReady -= InitializeCTS;
        }

        private bool CanTriggerInfluence(Collider2D collider)
        {
            return collider && collider.attachedRigidbody
                && collider.attachedRigidbody.linearVelocity.magnitude > foliageController.MinSpeedToTrigger;

        }

        private async void OnTriggerEnter2D(Collider2D collider)
        {
            if (!easingIn && CanTriggerInfluence(collider))
            {
                await BeginInfluence(collider);
            }
        }

        private async void OnTriggerExit2D(Collider2D collider)
        {
            await EndInfluence(collider);
        }

        private async Task BeginInfluence(Collider2D collider)
        {
            influencingCollider = collider;

            float orientation = Mathf.Sign(collider.transform.localScale.x);
            if ((transform.position.x < collider.transform.position.x && orientation > 0)
                || (transform.position.x > collider.transform.position.x && orientation < 0))
            {
                orientation *= -1;
            }
            await TryEaseIn(collider.attachedRigidbody.linearVelocity, orientation);
        }

        private async Task EndInfluence(Collider2D collider)
        {
            if (collider == influencingCollider)
            {
                influencingCollider = null;
                if (!easingOut)
                {
                    await TryEaseOut();
                }
            }
        }

        private async Task EndCurrentInfluence()
        {
            if (influencingCollider != null)
            {
                await EndInfluence(influencingCollider);
            }
        }

        //Have to do this because TriggerEnter/Exit can get called even when game object is inactive,
        //and inactive game object can't start a coroutine (so without it we were getting a flood of errors every
        //time we exited play mode)
        private async Task TryEaseIn(Vector2 velocity, float orientation)
        {
            if (gameObject.activeInHierarchy)
            {
                await EaseIn(velocity, orientation, lifeCTS.Token);
                //StartCoroutine(EaseIn(velocity, orientation));
            }
        }

        private async Task TryEaseOut()
        {
            if (gameObject.activeInHierarchy)
            {
                await EaseOut(lifeCTS.Token);
                //StartCoroutine(EaseOut());
            }
        }

        private async Task EaseIn(Vector2 velocity, float orientation, CancellationToken token)
        {
            easingIn = true;
            easingOut = false;
            foliageController.SetFoliageInfluenceOrientation(foliageMaterial, orientation);

            velocity *= externalInfluenceStrength;
            float timer = 0;

            while (timer < foliageController.EaseInTime
                    && !token.IsCancellationRequested)
            {
                if (!easingIn)
                {
                    break;
                }
                float progress = timer / foliageController.EaseInTime;
                Vector2 newVelocity = (1 - progress) * defaultInfluence + progress * velocity;
                foliageController.SetFoliageInfluenceVelocity(foliageMaterial, newVelocity);
                timer += Time.deltaTime;
                await Task.Yield();
            }

            if (easingIn)
            {
                foliageController.SetFoliageInfluenceVelocity(foliageMaterial, velocity);
                easingIn = false;
            }
        }

        //IEnumerator EaseIn(Vector2 velocity, float orientation)
        //{
        //    easingIn = true;
        //    easingOut = false;
        //    foliageController.SetFoliageInfluenceOrientation(foliageMaterial, orientation);

        //    velocity *= externalInfluenceStrength;
        //    float timer = 0;

        //    while (timer < foliageController.EaseInTime)
        //    {
        //        if (!easingIn)
        //        {
        //            yield break;
        //        }
        //        float progress = timer / foliageController.EaseInTime;
        //        Vector2 newVelocity = (1 - progress) * defaultInfluence + progress * velocity;
        //        foliageController.SetFoliageInfluenceVelocity(foliageMaterial, newVelocity);
        //        timer += Time.deltaTime;
        //        yield return null;
        //    }

        //    if (easingIn)
        //    {
        //        foliageController.SetFoliageInfluenceVelocity(foliageMaterial, velocity);
        //        easingIn = false;
        //    }
        //}

        private async Task EaseOut(CancellationToken token)
        {
            easingOut = true;
            easingIn = false;

            Vector2 startingInfluence = foliageMaterial.GetVector(influenceVelocityProperty);
            float timer = 0;

            while (timer < foliageController.EaseOutTime && !token.IsCancellationRequested)
            {
                if (!easingOut)
                {
                    break;
                }
                float p = timer / foliageController.EaseOutTime;
                p = Mathf.Pow(p, snapBackRate);
                Vector2 newVelocity = (1 - p) * startingInfluence
                    + p * defaultInfluence;
                foliageController.SetFoliageInfluenceVelocity(foliageMaterial, newVelocity);
                timer += Time.deltaTime;
                await Task.Yield();
            }

            if (token.IsCancellationRequested)
            {
                Debug.Log("Ease out cancelled");
                return;
            }

            if (easingOut)
            {
                foliageController.SetFoliageInfluenceVelocity(foliageMaterial, defaultInfluence);
                easingOut = false;
            }
        }

        //IEnumerator EaseOut()
        //{
        //    easingOut = true;
        //    easingIn = false;

        //    Vector2 startingInfluence = foliageMaterial.GetVector(influenceVelocityProperty);
        //    float timer = 0;

        //    while (timer < foliageController.EaseOutTime)
        //    {
        //        if (!easingOut)
        //        {
        //            yield break;
        //        }
        //        float p = timer / foliageController.EaseOutTime;
        //        p = Mathf.Pow(p, snapBackRate);
        //        Vector2 newVelocity = (1 - p) * startingInfluence 
        //            + p * defaultInfluence;
        //        foliageController.SetFoliageInfluenceVelocity(foliageMaterial, newVelocity);
        //        timer += Time.deltaTime;
        //        yield return null;
        //    }

        //    if (easingOut)
        //    {
        //        foliageController.SetFoliageInfluenceVelocity(foliageMaterial, defaultInfluence);
        //        easingOut = false;
        //    }
        //}

        //private void OnDisable()
        //{
        //    StopAllCoroutines();
        //}

        private void OnDisable()
        {
            if (!lifeCTS.IsCancellationRequested)
            {
                lifeCTS.Cancel();
                lifeCTS.Dispose();
            }
            OnInfluencingColliderDisabled = null;
        }
    }
}