using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGPlatformer.Dialogue.Editor
{
    [CustomPropertyDrawer(typeof(DialogueActorData))]
    public class DialogueActorDataPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var lv = new ListView()
            {
                reorderable = false,
                showAddRemoveFooter = false,
                showFoldoutHeader = true
            };
            lv.BindProperty(property.FindPropertyRelative("actors"));
            lv.headerTitle = "Actors";
            return lv;
        }
    }
}