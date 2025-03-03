using UnityEngine;

namespace RPGPlatformer.Core
{
    [ExecuteInEditMode]
    public class SortingLayerControl : MonoBehaviour, SortingLayerDataSource
    {
        [SerializeField] SerializableSortingLayer sortingLayer;
        [SerializeField] int sortingOrder;

        public int? SortingLayerID
        {
            get
            {
                var layers = SortingLayer.layers;
                var layerNumber = Mathf.Clamp(sortingLayer.layerNumber, 0, layers.Length - 1);
                return layers[layerNumber].id;

            }
        }
        public int? SortingOrder => sortingOrder;

        private void OnValidate()
        {
            UpdateChildren();
        }

        public void UpdateChildren()
        {
            foreach (var child in GetComponentsInChildren<ChildSortingLayer>())
            {
                child.UpdateSortingData();
            }
        }
    }
}