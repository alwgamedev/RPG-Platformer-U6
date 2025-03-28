using RPGPlatformer.UI;
using System;
using System.Collections.Generic;

namespace RPGPlatformer.Core
{
    public interface IInteractableGameObject : IExaminable
    {
        public string DisplayName { get; }
        public bool MouseOver { get; }
        public CursorType CursorType { get; }

        public bool PlayerCanInteract();

        //(description, condition to execute, action)
        public IEnumerable<(string, Func<bool>, Action)> InteractionOptions();
    }
}