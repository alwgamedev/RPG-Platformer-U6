using UnityEngine;

namespace RPGPlatformer.Effects
{
    public class PoolableParticleEffect : PoolableEffect
    {
        [SerializeField] protected ParticleSystem particleEffect;

        public override float AutomaticRequeDelay => particleEffect.main.duration 
            + particleEffect.main.startDelay.constantMax
            + particleEffect.main.startLifetime.constantMax;

        protected override void PlayEffect()
        {
            if (particleEffect)
            {
                particleEffect.Play();
            }
        }

        protected override void StopEffect()
        {
            if (particleEffect)
            {
                particleEffect.Stop();
            }
        }
    }
}