using UnityEngine;

namespace RPGPlatformer.Effects
{
    public class PoolableAnimation : PoolableEffect
    {
        [SerializeField] protected Animation anim;

        public override float AutomaticRequeDelay => anim.clip.length;

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