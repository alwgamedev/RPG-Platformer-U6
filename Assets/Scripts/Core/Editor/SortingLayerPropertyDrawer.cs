using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.Core.Editor
{
    [CustomPropertyDrawer(typeof(SerializableSortingLayer))]
    public class SortingLayerPropertyDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var layerNumber = property.FindPropertyRelative("layerNumber");
            var sortingLayers = SortingLayer.layers.Select(x => x.name).ToArray();
            layerNumber.intValue = EditorGUI.Popup(position, label.text, layerNumber.intValue, sortingLayers);
        }
    }
}