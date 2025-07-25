﻿using System;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [ExecuteAlways]
    public class ChildSortingLayer : MonoBehaviour, SortingLayerDataSource
    {
        public int orderDelta;
        public Transform dataSource;

        SortingLayerDataSource slds;

        public event Action DataUpdated;
        public event Action Destroyed;

        public int? SortingLayerID
        {
            get
            {
                if (TryGetComponent(out Renderer renderer))
                {
                    return renderer.sortingLayerID;
                }
                if (slds != null)
                {
                    return slds.SortingLayerID;
                }
                return null;
            }
        }

        public int? SortingOrder
        {
            get
            {
                if (TryGetComponent(out Renderer renderer))
                {
                    return renderer.sortingOrder;
                }
                if (slds != null && slds.SortingOrder.HasValue)
                {
                    return slds.SortingOrder.Value + orderDelta;
                }
                return null;
            }
        }

        private void Awake()
        {
            UpdateSLDS();
        }

        private void OnValidate()
        {
            Validate();   
        }

        public void Validate()
        {
            UpdateSLDS();
            UpdateSortingData();
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
            if (!gameObject || !layerID.HasValue || !layerOrder.HasValue) return;

            if (TryGetComponent(out Renderer renderer))
            {
                renderer.sortingLayerID = layerID.Value;
                renderer.sortingOrder = layerOrder.Value + orderDelta;
            }
        }

        public void UpdateSLDS()
        {
            UnhookCurrentSLDS();

            if (dataSource == null) return;

            SortingLayerDataSource s = dataSource.GetComponent<SortingLayerControl>();
            //for game objects that have both an SLC and CSL, we prioritize using the SLC as the data source
            //so that a CLS can also have a SLC component attached and use that as its data source

            if (s == null)
            {
                s = dataSource.GetComponent<SortingLayerDataSource>();
            }

            HookUpNewSLDS(s);
        }

        private void UnhookCurrentSLDS()
        {
            if (slds == null || (slds is MonoBehaviour s && !s))
            {
                slds = null;
                return;
            }

            slds.DataUpdated -= UpdateSortingData;
            slds.Destroyed -= OnDataSourceDestroyed;
            slds = null;
        }

        private void HookUpNewSLDS(SortingLayerDataSource s)
        {
            if (s == null || (s is MonoBehaviour t && !t)) return;
            slds = s;

            if (slds != null && slds != (SortingLayerDataSource)this)
            {
                slds.DataUpdated += UpdateSortingData;
                slds.Destroyed += OnDataSourceDestroyed;
                UpdateSortingData();
            }
        }

        private void OnDataSourceDestroyed()
        {
            dataSource = null;
            UnhookCurrentSLDS();
        }

        private void OnDestroy()
        {
            UnhookCurrentSLDS();
            Destroyed?.Invoke();

            DataUpdated = null;
            Destroyed = null;
        }
    }
}