using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGPlatformer.Core.Editor
{
    [CustomPropertyDrawer(typeof(LabelledUnityObject<>), true)]
    public class LabelledUnityObjectPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var v = new PropertyField(property.FindPropertyRelative("labelledObject"));
            v.label = property.FindPropertyRelative("label").stringValue;
            return v;
        }
    }
}