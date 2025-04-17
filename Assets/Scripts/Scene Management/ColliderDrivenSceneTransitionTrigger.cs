using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class ColliderDrivenSceneTransitionTrigger : MonoBehaviour, ISceneTransitionTrigger
    {
        [SerializeField] SceneTransitionTriggerData sceneTransition;

        bool transitionTriggered;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (transitionTriggered || !gameObject.activeInHierarchy) return;

            if (collider.gameObject.CompareTag("Player"))
            {
                transitionTriggered = true;
                ((ISceneTransitionTrigger)this).TriggerScene(sceneTransition);
            }
        }
    }
}