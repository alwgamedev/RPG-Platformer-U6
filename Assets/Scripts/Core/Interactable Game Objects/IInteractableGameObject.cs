using RPGPlatformer.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.Core
{
    public interface IInteractableGameObject : IExaminable, IDisplayNameSource
    {
        public Transform transform { get; }
        //public string DisplayName { get; }
        public bool MouseOver { get; }
        public CursorType CursorType { get; }
        public bool PlayerHasInteracted { get; }

        public bool PlayerCanInteract();

        //(description, condition to execute, action)
        public IEnumerable<(string, Func<bool>, Action)> InteractionOptions();
    }
}