using System;
using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [ExecuteAlways]
    public class VisualCurvePoint : MonoBehaviour
    {
        [SerializeField] SerializableTuple<Transform> data;

        public SerializableTuple<Transform> Data => data;

        public static event Action CurvePointMoved;

        private void Update()
        {
            if (transform.hasChanged)
            {
                CurvePointMoved?.Invoke();
            }

            if (data?.Item1 == null || data.Item2 == null) return;

            if (data.Item1.hasChanged || data.Item2.hasChanged)
            {
                CurvePointMoved?.Invoke();
            }
        }
    }
}