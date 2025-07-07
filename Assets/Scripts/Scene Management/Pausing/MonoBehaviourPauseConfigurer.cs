using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class MonoBehaviourPauseConfigurer : MonoBehaviour
    {
        Action OnDestroyed;

        private void Start()
        {
            SubscribeToPauseManager(PauseManager.Instance);
        }

        private void SubscribeToPauseManager(PauseManager pm)
        {
            if (!pm)
            {
                Debug.LogWarning($"{GetType().Name} on {gameObject.name} was given a null {typeof(PauseManager).Name}");
                return;
            }

            OnDestroyed = () => UnsubscribeFromPauseManager(pm);

            IPausable[] pausables = GetComponents<IPausable>();

            foreach (var pausable in pausables)
            {
                pm.OnPause += pausable.Pause;
                pm.OnUnpause += pausable.Unpause;
            }
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