using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class ColliderDrivenScenePortal : MonoBehaviour, ISceneTransitionTrigger
    {
        [SerializeField] SceneTransitionTriggerData sceneTransition;

        bool transitionTriggered;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (transitionTriggered || !gameObject.activeInHierarchy) return;

            if (collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                transitionTriggered = true;
                ((ISceneTransitionTrigger)this).TriggerScene(sceneTransition);
            }
        }
    }
}