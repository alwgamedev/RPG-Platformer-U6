using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGPlatformer.Dialogue.Editor
{
    [CustomPropertyDrawer(typeof(ResponseChoiceData))]
    public class ResponseChoiceDataPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var choiceField = new TextField("Response:", 1000, true, false, '*');
            choiceField.bindingPath = "choiceText";
            choiceField.ElementAt(0).style.minWidth = 75;//make the label a little smaller
            choiceField.style.width = 275;
            choiceField.style.maxHeight = 75;
            choiceField.multiline = true;
            choiceField.style.whiteSpace = WhiteSpace.Normal;
            container.Add(choiceField);

            var responseActions
                = new PropertyField(property.FindPropertyRelative("responseActions"), "Response Actions");
            container.Add(responseActions);

            return container;

        }
    }
}