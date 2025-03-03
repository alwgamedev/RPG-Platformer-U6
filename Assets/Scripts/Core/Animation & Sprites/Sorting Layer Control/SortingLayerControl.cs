using UnityEngine;

namespace RPGPlatformer.Core
{
    [ExecuteInEditMode]
    public class SortingLayerControl : MonoBehaviour, SortingLayerDataSource
    {
        [SerializeField] SerializableSortingLayer sortingLayer;
        [SerializeField] int sortingOrder;

        //TO-DO: only updates sorting layers which are children of its transform. however other 
        //sprites may be dependent on this SLC without being a child of its transform
        //PUSH TO GITHUB BEFORE YOU DO THIS IN CASE YOU LOSE A BUNCH OF WORK
        //alternatively use an event all dependent sprites subscribe to

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