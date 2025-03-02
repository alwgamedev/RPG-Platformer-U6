using UnityEngine;

namespace RPGPlatformer.Core
{
    [ExecuteInEditMode]
    public class SortingLayerControl : MonoBehaviour
    {
        [SerializeField] SortingLayer sortingLayer;
        [SerializeField] int sortingOrder;

        private void OnValidate()
        {
            if (ParentHasControl()) return;

            foreach (var child in GetComponentsInChildren<ChildSortingLayer>())
            {
                child.SetSortingData(sortingLayer, sortingOrder);
            }
        }

        private bool ParentHasControl()
        {
            var parent = transform.parent;

            while (parent != null)
            {
                if (parent.TryGetComponent<SortingLayerControl>(out _)) return true;
                parent = parent.parent;
            }

            return false;
        }
    }
}