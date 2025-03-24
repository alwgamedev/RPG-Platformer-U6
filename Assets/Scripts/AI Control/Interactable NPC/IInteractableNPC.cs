using RPGPlatformer.UI;
using System;
using System.Collections.Generic;

namespace RPGPlatformer.Core
{
    public interface IInteractableNPC : IInteractableGameObject
    {
        public IEnumerable<(string, Action)> GetInteractionOptions();

        public void SetCursorTypeAndPrimaryAction(CursorType cursorType, bool removePreviousFromOptions = true);
    }
}