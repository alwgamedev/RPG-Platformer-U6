using RPGPlatformer.UI;

namespace RPGPlatformer.Core
{
    public interface IInteractableGameObject : IExaminable
    {
        public string DisplayName { get; }
        public bool MouseOver { get; }
        public CursorType CursorType { get; }

        public bool PlayerCanInteract();
    }
}