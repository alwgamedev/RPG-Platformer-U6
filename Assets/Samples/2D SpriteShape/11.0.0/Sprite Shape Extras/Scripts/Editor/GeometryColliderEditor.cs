using UnityEditor;
using UnityEngine;

namespace SpriteShapeExtras.Editor
{
    [CustomEditor(typeof(GeometryCollider))]
    [CanEditMultipleObjects]
    public class GeometryColliderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Bake Collider"))
            {
                foreach (var t in targets)
                {
                    ((GeometryCollider)t).ForceBake();
                }
            }
        }
    }
}