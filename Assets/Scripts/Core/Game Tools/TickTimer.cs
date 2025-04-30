using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class TickTimer : MonoBehaviour//will be part of persistent objects
    {
        [SerializeField] protected float tickLength = 0.06f;
        [SerializeField] bool randomizeStartValue;//just public because CombatController sets it to true

        protected float tickTime;

        public float TickLength => tickLength;
        public float TickTime => tickTime;

        public event Action NewTick;

        private void Start()
        {
            if (randomizeStartValue)
            {
                AddRandomizedOffset();
            }
        }

        private void Update()
        {
            tickTime += Time.deltaTime;
            if (tickTime > tickLength)
            {
                tickTime -= tickLength;
                NewTick?.Invoke();
            }
        }

        public void AddRandomizedOffset()
        {
            tickTime += UnityEngine.Random.Range(0, tickLength);

            while (tickTime >= tickLength)
            {
                tickTime -= tickLength;
            }

            while(tickTime < 0)
            {
                tickTime += tickLength;
            }
        }

        private void OnDestroy()
        {
            NewTick = null;
        }
    }
}