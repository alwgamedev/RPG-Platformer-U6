using System;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public struct BreakData
    {
        public readonly BreakOptions options;
        public readonly Vector2 initialVelocity;
        public readonly Vector2 breakForce;
        public readonly Vector2 breakPoint;

        public BreakData(BreakOptions options, Vector2 initialVelocity, Vector2 breakForce, Vector2 breakPoint)
        {
            this.options = options;
            this.initialVelocity = initialVelocity;
            this.breakForce = breakForce;
            this.breakPoint = breakPoint;
        }
    }

    [Serializable]
    public struct BreakOptions
    {
        [SerializeField] bool inheritVelocity;
        [SerializeField] bool applyBreakForce;

        public bool InheritVelocity => inheritVelocity;
        public bool ApplyBreakForce => applyBreakForce;
    }
}