using UnityEngine;
using UnityEditor;

namespace RPGPlatformer.Core.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AutoSpriteShape))]
    public class AutoSpriteShapeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI ()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Generate Spline"))
            {
                foreach (var t in targets)
                {
                    ((AutoSpriteShape)t).GenerateSpline();
                }
            }
        }
    }
}