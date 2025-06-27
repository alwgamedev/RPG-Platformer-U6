using RPGPlatformer.UI;
using System;
using UnityEngine;

namespace RPGPlatformer.Combat
{
    [Serializable]
    public class ReplenishableStat
    {
        [SerializeField] float minValue;
        [SerializeField] float maxValue;
        [SerializeField] float defaultValue;
        [SerializeField] float replenishRate;//fraction of maxValue replenished per second

        public bool autoReplenish;
        public StatBarItem statBar;

        private float currentValue;

        public float MinValue => minValue;
        public float MaxValue => maxValue;
        public float DefaultValue => defaultValue;
        public float CurrentValue
        {
            get => currentValue;
            set
            {
                currentValue = value;
                if (statBar)
                {
                    statBar.SetFillAmount(currentValue / maxValue);
                    statBar.SetText($"{(int)currentValue} / {(int)maxValue}");
                }
            }
        }
        public float FractionOfMax
        {
            get
            {
                if (maxValue != 0)
                {
                    return currentValue / maxValue;
                }
                Debug.LogWarning($"Tried to retrieve fractional value from a replenishable stat, " +
                    $"but its max value is zero.");
                return 0;
            }
        }

        public event Action Depleted;

        public void SetMaxValue(float value, bool maintainFraction = false)
        {
            var f = FractionOfMax;
            maxValue = value;
            if (maintainFraction)
            {
                CurrentValue = f * maxValue;
            }
        }

        public void SetDefaultValue(float value)
        {
            defaultValue = value;
        }

        public void SetMaxAndDefaultValue(float value, bool maintainFraction = false)
        {
            SetDefaultValue(value);
            SetMaxValue(value, maintainFraction);
        }

        public void TakeDefaultValue()
        {
            CurrentValue = defaultValue;
        }

        //I don't check for depleted in CurrentValue.set,
        //so that we don't get depleted messages on initialization
        public void SetValueClamped(float value)
        {
            bool wasDepleted = CurrentValue <= minValue;
            CurrentValue = Mathf.Clamp(value, minValue, maxValue);
            if (CurrentValue <= minValue && !wasDepleted)
            {
                Depleted?.Invoke();
            }
        }

        public void AddValueClamped(float value)
        {
            SetValueClamped(CurrentValue + value);
        }

        public void AutoReplenish()
        {
            AddValueClamped(replenishRate * MaxValue * Time.deltaTime);
        }

        public void Update()
        {
            if (autoReplenish)
            {
                AutoReplenish();
            }
        }
    }
}