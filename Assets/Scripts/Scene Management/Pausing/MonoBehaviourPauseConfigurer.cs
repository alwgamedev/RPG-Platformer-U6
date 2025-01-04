using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class MonoBehaviourPauseConfigurer : MonoBehaviour
    {
        //List<IPausable> pauseable;
        Action OnDestroyed;
        bool subscribedToPauseManager;

        private void OnEnable()
        {
            if (subscribedToPauseManager) return;

            PauseManager pm = FindAnyObjectByType<PauseManager>();
            if (!pm) return;

            IPausable[] pausables = GetComponents<IPausable>();

            foreach (var pausable in pausables)
            {
                pm.OnPause += pausable.Pause;
                pm.OnUnpause += pausable.Unpause;
                OnDestroyed = () => UnsubscribeFromPauseManager(pm);
            }
            subscribedToPauseManager = true;
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