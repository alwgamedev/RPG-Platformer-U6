using UnityEngine;

namespace RPGPlatformer.UI
{
    public interface IDropTarget<T> where T : class
    {
        public Transform DraggableParentTransform { get; }
        public bool AllowReplacementIfCantSwap { get; }

        public bool CanPlace(T item, IDragSource<T> origin = null);
        //NOTE: it seems like we could just pass origin as the sole parameter and get item from origin.Contents()
        //but we just want to be extra sure origin's contents haven't changed

        public void PlaceItem(T item);

        public void DropComplete();
    }
}