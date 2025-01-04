using UnityEngine;

namespace RPGPlatformer.UI
{
    public interface IDropTarget<T> where T : class
    {
        public Transform Transform { get; }
        public bool AllowReplacementIfCantSwap { get; }

        public bool CanPlace(T item);

        public void PlaceItem(T item);

        public void DropComplete();
    }
}