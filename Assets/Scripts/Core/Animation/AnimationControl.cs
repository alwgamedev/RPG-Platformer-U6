using UnityEngine;

namespace RPGPlatformer.Core
{
    public class AnimationControl : MonoBehaviour//NOTE atm this has nothing to do with combat
    {
        public RuntimeAnimatorController defaultAnimatorController;
        public Animator animator;

        //TO-DO: add a get reference to the runtimeAnimatorController, so that movement can access its SetFloat, SetTrigger etc.,
        //and use this script (or a derived class) for movement (instead of everything having different, but essentially the same, animator scripts)

        private void Awake()
        {
            animator = GetComponent<Animator>();
            defaultAnimatorController = animator.runtimeAnimatorController;
        }

        public void SetAnimatorOverride(AnimatorOverrideController animOverride)
        {
            if (!animator) return;
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