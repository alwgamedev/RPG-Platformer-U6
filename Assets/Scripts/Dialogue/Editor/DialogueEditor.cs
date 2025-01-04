using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGPlatformer.Dialogue.Editor
{
    public class DialogueEditor : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Window/UI Toolkit/DialogueEditor")]
        public static void OpenWindow()
        {
            DialogueEditor wnd = GetWindow<DialogueEditor>();
            wnd.titleContent = new GUIContent("DialogueEditor");
        }

        public void CreateGUI()
        {
            AddGraphView();
        }

        private void AddGraphView()
        {
            DialogueEditorGraphView graphView = new DialogueEditorGraphView();
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }
    }
}
