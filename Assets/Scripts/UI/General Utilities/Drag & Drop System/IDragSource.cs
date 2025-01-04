using System;
using UnityEngine;

namespace RPGPlatformer.UI
{
    public interface IDragSource<T> where T : class
    {
        public Transform Transform { get; }

        public T Contents();
        public void RemoveItem();
        public void DragComplete();
    }
}