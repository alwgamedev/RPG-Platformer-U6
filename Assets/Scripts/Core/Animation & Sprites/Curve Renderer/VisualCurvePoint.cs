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

        public bool HasChanged()
        {
            if (data?.Item1 == null || data.Item2 == null) return false;

            return transform.hasChanged || data.Item1.hasChanged || data.Item2.hasChanged;
        }
    }
}