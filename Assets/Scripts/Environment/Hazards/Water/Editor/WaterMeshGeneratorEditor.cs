using UnityEditor;
using UnityEngine;

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