using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class MonoBehaviourPauseConfigurer : MonoBehaviour
    {
        Action OnDestroyed;

        private void Start()
        {
            SubscribeToPauseManager(FindAnyObjectByType<PauseManager>());
        }

        private void SubscribeToPauseManager(PauseManager pm)
        {
            if (!pm) return;

            IPausable[] pausables = GetComponents<IPausable>();

            foreach (var pausable in pausables)
            {
                pm.OnPause += pausable.Pause;
                pm.OnUnpause += pausable.Unpause;
                OnDestroyed = () => UnsubscribeFromPauseManager(pm);
                //^doing it this with OnDestroyed so that we unsubscribe from the correct PauseManager
                //(just in case anything weird happens where we get destroyed in a moment where there are
                //two PauseManagers)
            }
            //subscribedToPauseManager = true;
        }

        private void UnsubscribeFromPauseManager(PauseManager pm)
        {
            if (!pm) return;

            IPausable[] pausables = GetComponents<IPausable>();

            foreach (var pausable in pausables)
            {
                pm.OnPause -= pausable.Pause;
                pm.OnUnpause -= pausable.Unpause;
            }
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();

            OnDestroyed = null;
        }
    }
}