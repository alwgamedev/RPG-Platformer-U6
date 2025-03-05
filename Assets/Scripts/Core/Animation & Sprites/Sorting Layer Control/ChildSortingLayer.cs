using System;
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

        public event Action DataUpdated;

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
            UpdateSLC();

            UpdateSortingData();

            DataUpdated?.Invoke();
        }

        public void UpdateSortingData()
        {
            if (slds != null)
            {
                SetSortingData(slds.SortingLayerID, slds.SortingOrder);
            }

            DataUpdated?.Invoke(); 
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

        private void UpdateSLC()
        {
            if (slds != null)
            {
                slds.DataUpdated -= UpdateSortingData;
            }

            if (dataSource == null) return;

            //for parents that have both SLC and CSL, give priority to the SLC as the data source
            SortingLayerDataSource s = dataSource.GetComponent<SortingLayerControl>();

            if (s == null)
            {
                dataSource.TryGetComponent(out s);
            }

            slds = s;

            if (slds != null && slds != (SortingLayerDataSource)this)
            {
                slds.DataUpdated += UpdateSortingData;
            }
        }

        private void OnDestroy()
        {
            if (slds != null)
            {
                slds.DataUpdated -= UpdateSortingData;
            }
        }
    }
}