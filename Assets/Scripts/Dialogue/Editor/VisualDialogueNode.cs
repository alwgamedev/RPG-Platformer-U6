using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGPlatformer.Dialogue.Editor
{
    public class VisualDialogueNode : Node
    {
        public DialogueNode dialogueNode;
        public Port inputPort;
        public List<Port> outputPorts = new();
        public bool outPutPortsReady = false;
        public Toggle rootNodeToggle;
        //public bool ignoreRootNodeToggleValueChange;

        public event Action OutputPortsReady;

        public VisualDialogueNode(DialogueNode dialogueNode)
        {
            this.dialogueNode = dialogueNode;
            title = dialogueNode.name;
        }

        public void Redraw()
        {
            if (dialogueNode == null) return;

            RedrawTitleContainer();
            RedrawInputContainer();
            RedrawOutputContainer();
            RedrawExtensionsContainer();
        }

        private void RedrawTitleContainer()
        {
            titleContainer.Clear();
            titleContainer.style.flexDirection = FlexDirection.Column;

            Toggle toggle = new("Player speaking:")
            {
                value = dialogueNode.IsPlayerSpeaking()
            };
            toggle.style.unityTextAlign = TextAnchor.MiddleLeft;
            toggle.style.minWidth = 5;//label and the actual toggle
            toggle.ElementAt(0).style.fontSize = 16;
            toggle.RegisterValueChangedCallback((valueChangeEvent) =>
            {
                dialogueNode.SetIsPlayerSpeaking(valueChangeEvent.newValue);
            });


            rootNodeToggle = new("Root node:");
            toggle.style.unityTextAlign = TextAnchor.MiddleLeft;
            toggle.style.minWidth = 5;
            toggle.ElementAt(0).style.fontSize = 8;

            titleContainer.Insert(0, toggle);
            titleContainer.Insert(1, rootNodeToggle);
        }

        private void RedrawInputContainer()
        {
            inputContainer.Clear();

            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input,
                Port.Capacity.Multi, typeof(VisualDialogueNode));
            inputPort.portName = "Parent";
            this.inputPort = inputPort;

            inputContainer.Insert(0, inputPort);
        }

        private void RedrawOutputContainer()
        {
            outPutPortsReady = false;

            if (dialogueNode is ChoicesDialogueNode choicesNode)
            {
                RedrawChoicesOutputContainer(choicesNode);
            }
            else
            {
                outputContainer.Clear();
                outputContainer.Insert(0, CreateContinuationPort());
                outPutPortsReady = true;
                OutputPortsReady?.Invoke();
            }
        }

        private void RedrawChoicesOutputContainer(ChoicesDialogueNode choicesNode)
        {
            outputContainer.Clear();

            List<ResponseChoiceData> responseChoices = choicesNode.ResponseChoices();

            Foldout choicesFoldout = new()
            {
                text = "Response Choices"
            };

            Func<VisualElement> makeItem = () => 
            {
                VisualElement choiceContainer = new();

                TextField textField = new();
                textField.style.width = 150;
                textField.style.maxHeight = 50;
                textField.multiline = true;
                textField.style.whiteSpace = WhiteSpace.Normal;
                textField.RegisterValueChangedCallback((textChangeEvent) =>
                {
                    ListView listView = choicesFoldout[0] as ListView;
                    List<TextField> textFields = listView.Query<TextField>().ToList();
                    int currentIndex = textFields.IndexOf(textField);
                    if (currentIndex < 0) return;
                    choicesNode.ResponseChoices()[currentIndex].choiceText = textChangeEvent.newValue;
                    EditorUtility.SetDirty(dialogueNode);
                });

                choiceContainer.Insert(0, textField);
                choiceContainer.Insert(1, CreateContinuationPort());
                if(outputPorts.Count == responseChoices.Count)
                {
                    outPutPortsReady = true;
                    OutputPortsReady?.Invoke();
                }

                return choiceContainer;
            };

            Action<VisualElement, int> bindItem = (elmt, index) =>
            {
                if (elmt == null || choicesNode == null || choicesNode.ResponseChoices() == null) return;
                TextField tf = elmt.Q<TextField>();
                if (tf != null)
                {
                    tf.value = choicesNode.ResponseChoices()[index]?.choiceText ?? ""; 
                }
            };

            ListView listView = new(responseChoices, 40, makeItem, bindItem)
            {
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true
            };

            choicesFoldout.Insert(0, listView);
            outputContainer.Insert(0, choicesFoldout);
        }

        private Port CreateContinuationPort()
        {
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output,
                    Port.Capacity.Single, typeof(VisualDialogueNode));
            outputPort.portName = "Continuation";
            outputPorts.Add(outputPort);
            return outputPort;
        }

        private void RedrawExtensionsContainer()
        {
            extensionContainer.Clear();

            List<string> textSegments = dialogueNode.TextSegments();

            Foldout dialogueFoldout = new()
            {
                text = "Speaker Dialogue"
            };

            ListView listView = new();
            listView.itemsSource = textSegments;
            listView.makeItem = () =>
            {
                TextField textField = new TextField();
                textField.style.width = 250;
                textField.style.maxHeight = 50;
                textField.multiline = true;
                textField.style.whiteSpace = WhiteSpace.Normal;
                textField.RegisterValueChangedCallback((textChangeEvent) =>
                {
                    List<TextField> textFields = listView.Query<TextField>().ToList();
                    int currentIndex = textFields.IndexOf(textField);
                    if (currentIndex < 0) return;
                    dialogueNode.TextSegments()[currentIndex] = textChangeEvent.newValue;
                    EditorUtility.SetDirty(dialogueNode);
                });
                return textField;

            };
            listView.bindItem = (elmt, index) => (elmt as TextField).value = textSegments[index] ?? "";
            listView.fixedItemHeight = 60;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.showAddRemoveFooter = true;
            
            dialogueFoldout.Insert(0, listView);
            extensionContainer.Insert(0, dialogueFoldout);
            extensionContainer.style.backgroundColor = new Color(.15f, .15f, .15f, 1);

            RefreshExpandedState();
        }
    }
}