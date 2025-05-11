using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGPlatformer.Environment.Editor
{
    [CustomEditor(typeof(WaterMeshGenerator))]
    public class WaterMeshGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Generate Mesh"))
            {
                ((WaterMeshGenerator)target).GenerateMesh();
            }
        }
    }
}