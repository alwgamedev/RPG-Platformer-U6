using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class ColliderDrivenScenePortal : MonoBehaviour, ISceneTransitionTrigger
    {
        [SerializeField] SceneTransitionTriggerData sceneTransition;

        bool transitionTriggered;

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