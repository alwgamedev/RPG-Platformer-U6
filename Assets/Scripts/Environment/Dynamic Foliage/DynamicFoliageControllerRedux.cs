using System;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class DynamicFoliageControllerRedux : MonoBehaviour
    {
        [SerializeField] float easeInTime = .12f;//time to ease in from 0 influence to max influence
        [SerializeField] float easeOutTime = .35f;
        [SerializeField] bool randomizeEaseInTime = true;
        [SerializeField] bool randomizeEaseOutTime = true;
        //[SerializeField] float easeOutRate = .5f;
        //[SerializeField] float easeOutConst = 0.1f;
        [SerializeField] float maxInfluence = 30;//minInfluence will be assumed -maxInfluence

        Material foliageMaterial;
        int externalInfluenceProperty = Shader.PropertyToID("_ExternalInfluence");

        float startingInfluence;
        float goalInfluence;
        float currentInfluence;
        //float easeInRate;
        //float easeOutRate;
        float easeTime;
        float easeTimer;

        Action OnUpdate;

        //static System.Random rng = new();

        private void Awake()
        {
            if (randomizeEaseInTime)
            {
                easeInTime *= MiscTools.RandomFloat(0.75f, 1.25f);//(float)(0.5f * rng.NextDouble() + 0.75f);
            }

            if (randomizeEaseOutTime)
            {
                easeOutTime *= MiscTools.RandomFloat(0.75f, 1.25f);//(float)(0.5f * rng.NextDouble() + 0.75f);
            }

            //easeInRate = maxInfluence / easeInTime;
            //easeOutRate = maxInfluence / easeOutTime;
        }

        private void Start()
        {
            foliageMaterial = GetComponentInChildren<SpriteRenderer>().material;
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        public void BeginEaseIn(Vector2 velocity, float orientation)
        {
            startingInfluence = currentInfluence;
            SetGoalInfluence(velocity, orientation);
            easeTimer = 0;
            easeTime = easeInTime * Mathf.Abs(goalInfluence - currentInfluence) / maxInfluence;
            OnUpdate = LinearEase;//EaseIn;
        }

        public void SetGoalInfluence(Vector2 velocity, float orientation)
        {
            goalInfluence = Mathf.Clamp(orientation * (Mathf.Abs(velocity.x) + Mathf.Abs(velocity.y)),
                -maxInfluence, maxInfluence);
        }

        public void BeginEaseOut()
        {
            startingInfluence = currentInfluence;
            goalInfluence = 0;
            easeTimer = 0;
            easeTime = easeOutTime * Mathf.Abs(currentInfluence) / maxInfluence;
            OnUpdate = LinearEase;//EaseOut;
        }

        private void SetInfluence(float influence)
        {
            foliageMaterial.SetFloat(externalInfluenceProperty, influence);
        }

        //private void EaseIn()
        //{
        //    LinearEase();
        //}

        //private void EaseOut()
        //{
        //    LinearEase();
        //}

        private void LinearEase()
        {
            easeTimer += Time.deltaTime;
            currentInfluence = Mathf.Lerp(startingInfluence, goalInfluence, easeTimer / easeTime);
            if (Mathf.Abs(currentInfluence - goalInfluence) < 0.0001f)
            {
                CompleteEase();
            }
            else
            {
                SetInfluence(currentInfluence);
            }
        }

        //private void ExpEase(float rate, float constant)
        //{
        //    currentInfluence += (constant * Mathf.Sign(goalInfluence - currentInfluence) 
        //        + (goalInfluence - currentInfluence) * rate) * Time.deltaTime;
        //    //this gives it a more natural spring back
        //    if (Mathf.Abs(currentInfluence - goalInfluence) < 0.1f)
        //    {
        //        CompleteEase();
        //    }
        //    else
        //    {
        //        SetInfluence(currentInfluence);
        //    }
        //}

        private void CompleteEase()
        {
            SetInfluence(goalInfluence);
            currentInfluence = goalInfluence;
            OnUpdate = null;
        }

        private void OnDestroy()
        {
            OnUpdate = null;
        }
    }
}
