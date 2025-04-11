using RPGPlatformer.SceneManagement;
using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [RequireComponent(typeof(MonoBehaviorInputConfigurer))]
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class PlayerInputManager : MonoBehaviour, IInputSource, IPausable
    {
        public bool IsInputDisabled { get; private set; }

        public event Action InputEnabled;
        public event Action InputDisabled;

        public void EnableInput()
        {
            IsInputDisabled = false;
            InputEnabled?.Invoke();
        }

        public void DisableInput()
        {
            IsInputDisabled = true;
            InputDisabled?.Invoke();
        }

        public void Pause()
        {
            DisableInput();
        }

        public void Unpause()
        {
            EnableInput();
        }

        private void OnDestroy()
        {
            InputEnabled = null;
            InputDisabled = null;
        }
    }
}