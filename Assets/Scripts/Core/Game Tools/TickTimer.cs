using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public class TickTimer : MonoBehaviour//will be part of persistent objects
    {
        public readonly float TickLength = 0.06f;

        public bool RandomizeStartValue;

        public float TickCount { get; private set; } = 0;

        public event Action NewTick;

        private void Start()
        {
            if (RandomizeStartValue)
            {
                TickCount = UnityEngine.Random.Range(0, TickLength);
            }
        }

        private void Update()
        {
            TickCount += Time.deltaTime;
            if (TickCount > TickLength)
            {
                TickCount -= TickLength;
                NewTick?.Invoke();
            }
        }

        private void OnDestroy()
        {
            NewTick = null;
        }
    }
}