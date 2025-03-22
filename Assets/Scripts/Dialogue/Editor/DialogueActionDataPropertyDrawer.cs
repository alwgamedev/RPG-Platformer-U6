using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGPlatformer.Dialogue.Editor
{
    [CustomPropertyDrawer(typeof(DialogueActionData))]
    public class DialogueActionDataPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var nameField = new TextField("Action Name:", 100, true, false, '*');
            nameField.bindingPath = "actionName";
            nameField.ElementAt(0).style.minWidth = 75;//make the label a little smaller
            container.Add(nameField);

            var paramsField = new ListView()
            {
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showFoldoutHeader = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            paramsField.headerTitle = "Parameters";

            //so we get an unlabeled text field (otherwise says "Element 0, Element1, ..." in front
            //of each item, which takes up too much space)
            paramsField.makeItem = () =>
            {
                return new TextField();
            };

            paramsField.style.minWidth = 150;

            paramsField.BindProperty(property.FindPropertyRelative("parameters"));
            container.Add(paramsField);

            return container;
        }
    }
}