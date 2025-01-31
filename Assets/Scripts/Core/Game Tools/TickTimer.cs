using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class TickTimer : MonoBehaviour//will be part of persistent objects
    {
        [SerializeField] protected float tickLength = 0.06f;

        public bool randomizeStartValue;//just public because CombatController sets it to true

        protected float tickTime;

        public float TickTime => tickTime;

        public event Action NewTick;

        private void Start()
        {
            if (randomizeStartValue)
            {
                tickTime = UnityEngine.Random.Range(0, tickLength);
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

        private void OnDestroy()
        {
            NewTick = null;
        }
    }
}