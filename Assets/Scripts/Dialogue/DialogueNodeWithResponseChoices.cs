using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGPlatformer.Dialogue
{
    public class DialogueNodeWithResponseChoices : DialogueNode
    {
        [SerializeField] List<string> responseChoices = new();

        public List<string> ResponseChoices()
        {
            return responseChoices;
        }

        protected override void DrawOutputContainer()
        {
            Foldout choicesFoldout = new()
            {
                text = "Response Choices"
            };

            for (int i = 0; i < responseChoices.Count; i++)
            {
                Foldout choiceFoldout = new()
                {
                    text = $"Choice {i}"
                };

                TextField choiceText = new()
                {
                    value = responseChoices[i]
                };

                Port choiceOutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output,
                    Port.Capacity.Single, typeof(Node));
                choiceOutputPort.portName = "Continuation";

                Button deleteButton = new()
                {
                    text = "Delete"
                };

                choiceFoldout.Insert(0, choiceText);
                choiceFoldout.Insert(1, choiceOutputPort);
                choiceFoldout.Insert(2, deleteButton);
                choicesFoldout.Insert(i, choiceFoldout);
            }

            Button addButton = new();
            addButton.text = "Add";
            choicesFoldout.Insert(choicesFoldout.childCount, addButton);

            outputContainer.Insert(0, choicesFoldout);
        }
    }
}