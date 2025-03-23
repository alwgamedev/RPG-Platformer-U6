using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.VisualScripting;
using JetBrains.Annotations;
using Codice.Client.BaseCommands;

namespace RPGPlatformer.Dialogue.Editor
{
    public class VisualDialogueNode : Node
    {
        public List<string> actorNames;
        public DialogueNode dialogueNode;
        public Port inputPort;
        public Toggle rootNodeToggle;

        public List<Port> OutputPorts => outputContainer.Query<Port>().ToList();
        //easier than tracking ports getting added/deleted

        SerializedObject serObject;

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

        public bool NodeReady()
        {
            if (dialogueNode is ResponseChoicesDialogueNode r)
            {
                return OutputPorts.Count == r.ResponseChoices().Count;
            }

            if (dialogueNode is DecisionDialogueNode d)
            {
                return OutputPorts.Count == d.Continuations().Count;
            }

            return true;
        }

        private DropdownField ActorsDropDown(string label, int startingIndex)
        {
            var actorsDropDown = new DropdownField(actorNames, startingIndex);
            actorsDropDown.label = label;
            return actorsDropDown;
        }

        private void RedrawTitleContainer()
        {
            titleContainer.Clear();
            titleContainer.style.flexDirection = FlexDirection.Column;
            titleContainer.style.minHeight = 50;

            rootNodeToggle = new("Root node:");
            rootNodeToggle.ElementAt(0).style.fontSize = 11;
            rootNodeToggle.ElementAt(0).style.minWidth = 5;

            if (actorNames != null && actorNames.Count > 0)
            {
                var actorsDropDown = ActorsDropDown("Speaker Name", dialogueNode.SpeakerIndex());
                actorsDropDown.ElementAt(0).style.fontSize = 15;
                actorsDropDown.ElementAt(0).style.minWidth = 5;

                actorsDropDown.RegisterValueChangedCallback((valueChangeEvent) =>
                {
                    dialogueNode.SetSpeakerIndex(actorsDropDown.index);
                });
                titleContainer.Add(actorsDropDown);
            }
            else
            {
                var noActorsMsg = new Label("Add actor names to dialogue SO.");
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
            if (dialogueNode is ResponseChoicesDialogueNode choicesNode)
            {
                RedrawChoicesOutputContainer(choicesNode);
            }
            else if (dialogueNode is DecisionDialogueNode decisionNode)
            {
                RedrawDecisionOutputContainer(decisionNode);
            }
            else
            {
                outputContainer.Clear();
                outputContainer.Insert(0, CreateContinuationPort());
            }
        }

        private void RedrawDecisionOutputContainer(DecisionDialogueNode decisionNode)
        {
            var lv = new ListView()
            {
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showFoldoutHeader = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            lv.makeItem = () =>
            {
                var p = CreateContinuationPort();
                p.Q<Label>().style.width = 0;
                //we need the label because it stores the continuation id, but want to hide it bc looks ugly
                return p;
            };
            lv.BindProperty(serObject.FindProperty("continuations"));

            outputContainer.Add(lv);

            //have to do this bc if you set bindItem before BindProperty is all set up,
            //then it doesn't give you the good bindItem that does everything magically.
            //looks like stupid hackery, and it is
        }

        private void RedrawChoicesOutputContainer(ResponseChoicesDialogueNode choicesNode)
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

            outputContainer.Add(choicesList);

            float startTime = Time.realtimeSinceStartup;
            choicesList.schedule.Execute(() => { }).Until(LVReady);

            //have to do this bc if you set bindItem before BindProperty is all set up,
            //then it doesn't give you the good bindItem that does everything magically.
            //looks like stupid hackery, and it is
            bool LVReady()
            {
                if (Time.realtimeSinceStartup - startTime > 1)
                {
                    return true;
                }

                if (choicesList.bindItem == null)
                {
                    return false;
                }

                choicesList.bindItem += (elmt, i) =>
                {
                    if (elmt.Q<Port>() == null)
                    {
                        elmt.Add(CreateContinuationPort());
                    }
                };

                return true;
            }
        }

        private Port CreateContinuationPort()
        {
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output,
                    Port.Capacity.Single, typeof(VisualDialogueNode));
            outputPort.portName = "Continuation";
            return outputPort;
        }

        //to hide the continuation ID shown when you make a connection
        //(cheap workaround)
        //private Port DecisionContinuationPort()
        //{
        //    var p = CreateContinuationPort();colo
        //    return p;
        //}

        private void RedrawExtensionsContainer()
        {
            extensionContainer.Clear();

            var textSegments = new ListView()
            {
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showFoldoutHeader = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            textSegments.headerTitle = "Speaker Dialogue";
            textSegments.makeItem = () =>
            {
                var textField = new TextField();
                textField.style.width = 250;
                textField.ElementAt(0).style.minHeight = 50;
                textField.ElementAt(0).style.maxHeight = 125;
                textField.style.paddingBottom = 15;
                textField.style.paddingTop = 5;
                textField.multiline = true;
                textField.style.whiteSpace = WhiteSpace.Normal;
                return textField;

            };
            textSegments.BindProperty(serObject.FindProperty("textSegments"));

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

            extensionContainer.Add(textSegments);
            extensionContainer.Add(entryActions);
            extensionContainer.Add(exitActions);

            if (dialogueNode is DecisionDialogueNode decisionNode)
            {
                var dialogueFct = serObject.FindProperty("decisionFunctionData");
                //var actorIndex = dialogueFct.FindPropertyRelative("actorIndex");
                var functionData = dialogueFct.FindPropertyRelative("functionData");

                var container = new Foldout();
                container.text = "Decision Function";
                //var header = new Label("Decision Function");

                var actorsDropDown = ActorsDropDown("Actor", decisionNode.DecisionFunctionData().ActorIndex);
                actorsDropDown.RegisterValueChangedCallback((valueChangedEvent) =>
                {
                    decisionNode.SetDecisionActor(actorsDropDown.index);
                });

                var fctData = new PropertyField(functionData);
                fctData.BindProperty(functionData);

                //container.Add(header);
                container.Add(actorsDropDown);
                container.Add(fctData);

                container.Bind(serObject);

                extensionContainer.Add(container);

                //>this also works fine (below), but we don't get actor names
                //var decisionFunctionData = new PropertyField(serObject.FindProperty("decisionFunctionData"));
                //decisionFunctionData.BindProperty(serObject.FindProperty("decisionFunctionData"));
                //extensionContainer.Add(decisionFunctionData);
            }

            extensionContainer.style.backgroundColor = new Color(.15f, .15f, .15f, 1);

            RefreshExpandedState();
        }
    }
}