using UnityEngine;
using UnityEngine.U2D;

namespace RPGPlatformer.Core
{
    [ExecuteInEditMode]
    public class ChildSortingLayer : MonoBehaviour, SortingLayerDataSource
    {
        [SerializeField] bool ignoreParentSortingData;
        [SerializeField] int orderDelta;
        [SerializeField] Transform dataSource;

        SortingLayerDataSource slds;

        public int? SortingLayerID
        {
            get
            {
                if (TryGetComponent(out SpriteRenderer sr))
                {
                    return sr.sortingLayerID;
                }

                if (TryGetComponent(out SpriteShapeRenderer ssr))
                {
                    return ssr.sortingLayerID;
                }

                return null;
            }
        }

        public int? SortingOrder
        {
            get
            {
                if (TryGetComponent(out SpriteRenderer sr))
                {
                    return sr.sortingOrder;
                }

                if (TryGetComponent(out SpriteShapeRenderer ssr))
                {
                    return ssr.sortingOrder;
                }

                return null;
            }
        }

        private void OnValidate()
        {
            UpdateSortingData();
        }

        public void UpdateSortingData()
        {
            if (dataSource == null)
            {
                Debug.LogWarning($"{GetType().Name} component on {gameObject.name} is missing a " +
                    $"sorting layer data source (Transform).");
                return;
            }

            dataSource.TryGetComponent(out slds);

            if (ignoreParentSortingData) return;

            if (slds == null)
            {
                Debug.LogWarning($"{GetType().Name} component on {gameObject.name} has a reference to a " +
                    $"transform for its sorting layer data source, but that transform does not " +
                    $"have a SortingLayerDataSource component.");
                return;
            }

            SetSortingData(slds.SortingLayerID, slds.SortingOrder);
        }

        private void SetSortingData(int? layerID, int? layerOrder)
        {
            if (ignoreParentSortingData) return;

            if (!layerID.HasValue || !layerOrder.HasValue) return;

            if (TryGetComponent(out SpriteRenderer sr))
            {
                sr.sortingLayerID = layerID.Value;
                sr.sortingOrder = layerOrder.Value + orderDelta;
            }

            if (TryGetComponent(out SpriteShapeRenderer ssr))
            {
                ssr.sortingLayerID = layerID.Value;
                ssr.sortingOrder = layerOrder.Value + orderDelta;
            }
        }
    }
}