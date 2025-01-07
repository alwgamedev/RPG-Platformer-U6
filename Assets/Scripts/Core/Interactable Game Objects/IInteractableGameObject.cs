namespace RPGPlatformer.Core
{
    public interface IInteractableGameObject : IExaminable
    {
        public string DisplayName { get; }
        public bool MouseOver { get; }
    }
}