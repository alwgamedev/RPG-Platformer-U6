using System;

namespace RPGPlatformer.Core
{
    public interface IInputSource
    {
        public bool IsInputDisabled { get; }

        public event Action InputEnabled;
        public event Action InputDisabled;

        public void EnableInput();
        public void DisableInput();
    }
}