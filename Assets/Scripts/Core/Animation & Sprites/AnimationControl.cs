using UnityEngine;

namespace RPGPlatformer.Core
{
    public class AnimationControl : MonoBehaviour//NOTE atm this has nothing to do with combat
    {
        public RuntimeAnimatorController defaultAnimatorController;
        public Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            defaultAnimatorController = animator.runtimeAnimatorController;
        }

        public void SetAnimatorOverride(AnimatorOverrideController animOverride)
        {
            if (!animator || animOverride == null) return;
            animator.runtimeAnimatorController = animOverride;
            //I believe setting to null will default back to the original controller
        }

        public void RevertAnimatorOverride()
        {
            if (!animator) return;
            animator.runtimeAnimatorController = defaultAnimatorController;
        }

        public void PlayIdleAnimation()
        {
            PlayAnimationState("Idle", "Base Layer", 0);
        }

        public void PlayAnimationState(string stateName, string layerName, float normalizedTime)
        {
            if (!animator) return;
            animator.Play(stateName, animator.GetLayerIndex(layerName), normalizedTime);
        }

        public void Freeze(bool val)//true => freeze animator, false => unfreeze animator
        {
            if (!animator) return;
            animator.enabled = !val;
        }
    }
}