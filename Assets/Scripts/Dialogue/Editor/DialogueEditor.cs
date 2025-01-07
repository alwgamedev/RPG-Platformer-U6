using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;

namespace RPGPlatformer.Dialogue.Editor
{
    public class DialogueEditor : EditorWindow
    {
        DialogueSO selectedDialogue;
        DialogueEditorGraphView dialogueGraphView;

        [MenuItem("Window/UI Toolkit/DialogueEditor")]
        public static void OpenWindow()
        {
            GetWindow<DialogueEditor>(false, "Dialogue Editor");
        }

        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            DialogueSO dialogue = EditorUtility.InstanceIDToObject(instanceID) as DialogueSO;
            if (dialogue != null)
            {
                OpenWindow();
                return true;
            }
            return false;
        }

        public void CreateGUI()
        {
            dialogueGraphView = AddGraphView();
        }

        private void OnSelectionChange()
        {
            DialogueSO dialogue = Selection.activeObject as DialogueSO;
            selectedDialogue = dialogue;
            dialogueGraphView.DisplayDialogue(selectedDialogue);
        }

        private DialogueEditorGraphView AddGraphView()
        {
            DialogueEditorGraphView graphView = new DialogueEditorGraphView();
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
            return graphView;
        }
    }
}
