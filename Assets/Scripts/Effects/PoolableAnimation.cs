using UnityEngine;

namespace RPGPlatformer.Effects
{
    public class PoolableAnimation : PoolableEffect
    {
        [SerializeField] protected Animation anim;

        public override float AutomaticRequeDelay => anim.clip.length;

        public override void BeforeSetActive() { }

        public override void AfterSetActive() { }

        public override void Configure(object parameters) { }

        protected override void PlayEffect()
        {
            if (anim)
            {
                anim.Play();
            }
        }

        protected override void StopEffect()
        {
            if (anim)
            {
                anim.Stop();
            }
        }
    }
}