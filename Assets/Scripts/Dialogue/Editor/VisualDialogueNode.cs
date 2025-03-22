using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace RPGPlatformer.Dialogue.Editor
{
    public class VisualDialogueNode : Node
    {
        public List<string> actorNames;
        public DialogueNode dialogueNode;
        public Port inputPort;
        public List<Port> outputPorts = new();
        public bool outputPortsReady = false;
        public Toggle rootNodeToggle;
        //public bool ignoreRootNodeToggleValueChange;

        SerializedObject serObject;
        //ResponseChoiceDataPropertyDrawer responseChoiceDrawer = new();

        public event Action OutputPortsReady;

        public VisualDialogueNode(DialogueNode dialogueNode, List<string> actorNames)
        {
            this.dialogueNode = dialogueNode;
            title = dialogueNode.name;
            this.actorNames = actorNames;
            if (dialogueNode != null)
            {
                serObject = new(dialogueNode);
            }
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
            titleContainer.style.minHeight = 50;

            rootNodeToggle = new("Root node:");
            //rootNodeToggle.style.unityTextAlign = TextAnchor.UpperLeft;
            rootNodeToggle.ElementAt(0).style.fontSize = 11;
            rootNodeToggle.ElementAt(0).style.minWidth = 5;

            if (actorNames != null && actorNames.Count > 0)
            {
                var actorDropDown = new DropdownField(actorNames, dialogueNode.SpeakerIndex());
                actorDropDown.label = "Speaker Name:";
                //actorDropDown.style.unityTextAlign = TextAnchor.UpperLeft;
                actorDropDown.ElementAt(0).style.fontSize = 15;
                actorDropDown.ElementAt(0).style.minWidth = 5;
                actorDropDown.RegisterValueChangedCallback((valueChangeEvent) =>
                {
                    dialogueNode.SetSpeakerIndex(actorDropDown.index);
                });
                titleContainer.Add(actorDropDown);
            }
            else
            {
                var noActorsMsg = new Label("Add actor names to dialogue!");
                //noActorsMsg.style.unityTextAlign = TextAnchor.UpperLeft;
                noActorsMsg.style.fontSize = 15;
                titleContainer.Add(noActorsMsg);
            }

            titleContainer.Add(rootNodeToggle);
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
            outputPortsReady = false;

            if (dialogueNode is ChoicesDialogueNode choicesNode)
            {
                RedrawChoicesOutputContainer(choicesNode);
            }
            else
            {
                outputContainer.Clear();
                outputContainer.Insert(0, CreateContinuationPort());
                outputPortsReady = true;
                OutputPortsReady?.Invoke();
            }
        }

        private void RedrawChoicesOutputContainer(ChoicesDialogueNode choicesNode)
        {
            outputContainer.Clear();

            outputContainer.style.flexDirection = FlexDirection.Row;

            var choicesList = new ListView()
            {
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showFoldoutHeader = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            choicesList.style.minWidth = 250;
            choicesList.style.maxWidth = 325;
            choicesList.headerTitle = "Response Choices";
            choicesList.BindProperty(serObject.FindProperty("responseChoices"));

            choicesList.schedule.Execute(() => { }).Until(LVReady);

            //have to do this bc if you set bindItem before BindProperty is all set up,
            //then it doesn't give you the good bindItem that does everything magically.
            //looks like stupid hackery, and it is
            bool LVReady()
            {
                var lv = choicesList.Q<ListView>();

                if (lv == null || lv.bindItem == null)
                    return false;

                lv.bindItem += (elmt, i) =>
                {
                    if (elmt.Q<Port>() == null)
                    {
                        elmt.Add(CreateContinuationPort());
                    }

                    if (outputPortsReady == false
                        && lv.Query<Port>().ToList().Count >= choicesNode.ResponseChoices().Count)
                    {
                        outputPortsReady = true;
                        OutputPortsReady?.Invoke();
                    }
                };

                return true;
            }

            outputContainer.Add(choicesList);
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
                var textField = new TextField();
                textField.style.width = 250;
                textField.style.maxHeight = 200;
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

            void BindItem(VisualElement elmt, int index)
            {
                (elmt as TextField).value = dialogueNode.TextSegments()?[index] ?? "";
            }

            ListView listView = new(textSegments, 60, MakeItem, BindItem)
            {
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true
            };

            listView.itemsAdded += (args) => EditorUtility.SetDirty(dialogueNode);
            listView.itemsRemoved += (args) => EditorUtility.SetDirty(dialogueNode);

            var entryActions = new ListView()
            {
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showFoldoutHeader = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            entryActions.headerTitle = "Node Entry Actions";
            entryActions.style.minWidth = 250;
            entryActions.BindProperty(serObject.FindProperty("entryActions"));

            var exitActions = new ListView()
            {
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showFoldoutHeader = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            exitActions.headerTitle = "Node Exit Actions";
            exitActions.style.minWidth = 250;
            exitActions.BindProperty(serObject.FindProperty("exitActions"));

            dialogueFoldout.Add(listView);
            dialogueFoldout.Add(entryActions);
            dialogueFoldout.Add(exitActions);
            extensionContainer.Insert(0, dialogueFoldout);
            extensionContainer.style.backgroundColor = new Color(.15f, .15f, .15f, 1);

            RefreshExpandedState();
        }
        }
    }