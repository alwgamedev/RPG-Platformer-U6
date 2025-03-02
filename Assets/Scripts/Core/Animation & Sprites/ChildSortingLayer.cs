using UnityEngine;

namespace RPGPlatformer.Core
{
    public class ChildSortingLayer : MonoBehaviour
    {
        [SerializeField] bool ignoreParentSortingData;
        [SerializeField] int orderDelta;

        public void SetSortingData(SortingLayer parentLayer, int parentOrder)
        {
            if (ignoreParentSortingData) return;

            if (TryGetComponent(out SpriteRenderer sr))
            {
                sr.sortingLayerID = parentLayer.id;
                sr.sortingOrder = parentOrder + orderDelta;
            }
        }
    }
}