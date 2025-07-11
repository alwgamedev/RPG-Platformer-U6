using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.Core.Editor
{
    [CustomEditor(typeof(ColorControl))]
    public class ColorControlEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Set Child Colors"))
            {
                ((ColorControl)target).UpdateChildColors();
            }
            if (GUILayout.Button("Have Children Determine Shift & Multiplier"))
            {
                ((ColorControl)target).RequestChildrenAutoDetermineData();
            }
        }
    }
}