using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.Core.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PolygonColliderRefresher))]
    public class PolygonColliderRefresherEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if(GUILayout.Button("Refresh Collider"))
            {
                foreach (var t in targets)
                {
                    ((PolygonColliderRefresher)t).GenerateCollider();
                }
            }
        }
    }
}