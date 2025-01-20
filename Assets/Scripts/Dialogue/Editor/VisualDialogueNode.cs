using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace RPGPlatformer.Dialogue.Editor
{
    public class VisualDialogueNode : Node
    {
        public int numConversants;
        public DialogueNode dialogueNode;
        public Port inputPort;
        public List<Port> outputPorts = new();
        public bool outPutPortsReady = false;
        public Toggle rootNodeToggle;
        //public bool ignoreRootNodeToggleValueChange;

        public event Action OutputPortsReady;

        public VisualDialogueNode(DialogueNode dialogueNode, int numConversants)
        {
            this.dialogueNode = dialogueNode;
            title = dialogueNode.name;
            this.numConversants = numConversants;
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

            //Toggle toggle = new("Player speaking:")
            //{
            //    value = dialogueNode.IsPlayerSpeaking()
            //};
            //toggle.style.unityTextAlign = TextAnchor.MiddleLeft;
            //toggle.style.minWidth = 5;//label and the actual toggle
            //toggle.ElementAt(0).style.fontSize = 11;
            //toggle.RegisterValueChangedCallback((valueChangeEvent) =>
            //{
            //    dialogueNode.SetIsPlayerSpeaking(valueChangeEvent.newValue);
            //});

            List<string> conversantOptions = Enumerable.Range(0, numConversants).Select(x => x.ToString()).ToList();
            if (dialogueNode.ConversantNumber() < 0 || dialogueNode.ConversantNumber() >= numConversants)
            {
                dialogueNode.SetConversantNumber(0);
            }

            DropdownField conversantDropdown = new(conversantOptions, dialogueNode.ConversantNumber());
            conversantDropdown.label = "Conversant #:";
            conversantDropdown.style.unityTextAlign = TextAnchor.MiddleLeft;
            conversantDropdown.style.minWidth = 5;
            conversantDropdown.ElementAt(0).style.fontSize = 15;
            conversantDropdown.RegisterValueChangedCallback((valueChangeEvent) =>
            {
                dialogueNode.SetConversantNumber(Int32.Parse(valueChangeEvent.newValue));
            });


            rootNodeToggle = new("Root node:");
            rootNodeToggle.style.unityTextAlign = TextAnchor.MiddleLeft;
            rootNodeToggle.style.minWidth = 15;
            rootNodeToggle.ElementAt(0).style.fontSize = 11;

            titleContainer.Insert(0, conversantDropdown);
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

            VisualElement MakeItem()
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
                    choicesNode.SetResponseChoiceText(currentIndex, textChangeEvent.newValue);
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

            void BindItem(VisualElement elmt, int index)
            {
                if (elmt == null || choicesNode == null || choicesNode.ResponseChoices() == null) return;
                TextField tf = elmt.Q<TextField>();
                if (tf != null)
                {
                    tf.value = choicesNode.ResponseChoices()[index]?.choiceText ?? ""; 
                }
            };

            ListView listView = new(responseChoices, 60, MakeItem, BindItem)
            {
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true
            };

            listView.itemsAdded += (args) => EditorUtility.SetDirty(choicesNode);
            listView.itemsRemoved += (args) => EditorUtility.SetDirty(choicesNode);

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

            VisualElement MakeItem()
            {
                TextField textField = new TextField();
                textField.style.width = 250;
                textField.style.maxHeight = 50;
                textField.multiline = true;
                textField.style.whiteSpace = WhiteSpace.Normal;
                textField.RegisterValueChangedCallback((textChangeEvent) =>
                {
                    ListView listView = dialogueFoldout[0] as ListView;
                    List<TextField> textFields = listView.Query<TextField>().ToList();
                    int currentIndex = textFields.IndexOf(textField);
                    dialogueNode.SetTextSegment(currentIndex, textChangeEvent.newValue);
                });
                return textField;

            };

            void BindItem(VisualElement elmt, int index) => (elmt as TextField).value = textSegments[index] ?? "";

            ListView listView = new(textSegments, 60, MakeItem, BindItem)
            {
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true
            };

            listView.itemsAdded += (args) => EditorUtility.SetDirty(dialogueNode);
            listView.itemsRemoved += (args) => EditorUtility.SetDirty(dialogueNode);

            dialogueFoldout.Insert(0, listView);
            extensionContainer.Insert(0, dialogueFoldout);
            extensionContainer.style.backgroundColor = new Color(.15f, .15f, .15f, 1);

            RefreshExpandedState();
        }
    }
}