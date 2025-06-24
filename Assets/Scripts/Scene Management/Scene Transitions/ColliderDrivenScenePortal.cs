using RPGPlatformer.Core;
using RPGPlatformer.Saving;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    //you can also just not attach a collider if you want a scene trigger that will be set manually
    public class ColliderDrivenScenePortal : MonoBehaviour, ISceneTransitionTrigger
    {
        [SerializeField] SceneTransitionTriggerData sceneTransition;

        protected bool transitionTriggered;

        public bool TransitionTriggered => transitionTriggered;

        public void TriggerSceneTransition()
        {
            transitionTriggered = true;
            ((ISceneTransitionTrigger)this).TriggerScene(sceneTransition);
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (transitionTriggered || !gameObject.activeInHierarchy) return;

            if (collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                TriggerSceneTransition();
            }
        }
    }
}