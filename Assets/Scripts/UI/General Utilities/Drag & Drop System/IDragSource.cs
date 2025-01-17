using System;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public interface IDragSource<T> where T : class
    {
        public Transform DraggableParentTransform { get; }

        public T Contents();
        public bool ItemCanBeDragged();
        public void RemoveItem();
        public void DragComplete();
    }
}