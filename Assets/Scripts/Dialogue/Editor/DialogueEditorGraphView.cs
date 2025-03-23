using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System.Linq;
using RPGPlatformer.Core.Editor;

namespace RPGPlatformer.Dialogue.Editor
{
    public class DialogueEditorGraphView : GraphView
    {
        DialogueSO dialogue;

        Dictionary<DialogueNode, VisualDialogueNode> FindVisualNode = new();

        public DialogueEditorGraphView()
        {
            AddManipulators();
            AddGridBackground();
            AddStyles();

            graphViewChanged += OnGraphViewChanged;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter adapter)
        {
            return ports.Where(p => p.direction != startPort.direction && p.node != startPort.node).ToList();
        }

        public void DisplayDialogue(DialogueSO dialogue)
        {
            this.dialogue = null;//so that OnGraphViewChanged doesn't affect selected dialogue
            DeleteElements(graphElements);
            FindVisualNode.Clear();

            if (dialogue == null) return;

            this.dialogue = dialogue;

            foreach (var node in dialogue.Nodes())
            {
                if (node == null) return;

                var vNode = DrawNode(node, node.Rect().position);
                if(dialogue.RootNode() == node)
                {
                    vNode.rootNodeToggle.value = true;
                }
                FindVisualNode[node] = vNode;
            }
            foreach(var entry in FindVisualNode)//do this after all nodes have been added to the dictionary
                //so that we don't get an early complete when only one node is in there
            {
                var vNode = entry.Value;
                vNode.rootNodeToggle.RegisterValueChangedCallback(((valueChangeEvent) =>
                {
                    OnRootNodeToggleChanged(vNode);
                }));
                //entry.Value.OutputPortsReady += OutputPortsReadyHandler;
            }

            float startTime = Time.realtimeSinceStartup;
            schedule.Execute(() => { }).Until(AllPortsReady);

            bool AllPortsReady()
            {
                if (Time.realtimeSinceStartup - startTime > 1)
                {
                    return true;
                }

                if (FindVisualNode.Count != dialogue.Nodes().Count())
                {
                    return false;
                }

                foreach (var entry in FindVisualNode)
                {
                    if (!entry.Value.NodeReady())
                    {
                        return false;
                    }
                }

                DrawEdges(dialogue, FindVisualNode);
                return true;
            }
        }

        private void OnRootNodeToggleChanged(VisualDialogueNode vNode)
        {
            if (vNode.rootNodeToggle.value == false && dialogue.RootNode() == vNode.dialogueNode)
            {
                dialogue.SetRootNode(null);
                return;
            }
            if (vNode.rootNodeToggle.value == true)
            {
                dialogue.SetRootNode(vNode.dialogueNode);
                foreach (var other in FindVisualNode)
                {
                    if (other.Value != vNode)
                    {
                        other.Value.rootNodeToggle.value = false;
                    }
                }

            }
        }

        private void DrawEdges(DialogueSO dialogue, Dictionary<DialogueNode, VisualDialogueNode> FindVisualNode)
        {
            foreach (var node in FindVisualNode.Values)
            {
                for (int i = 0; i < node.OutputPorts.Count; i++)
                {
                    if (dialogue.TryGetContinuation(node.dialogueNode, i, out var continuationNode))
                    {
                        if (FindVisualNode.TryGetValue(continuationNode, out var visualContinuation))
                        {
                            var edge = node.OutputPorts[i].ConnectTo(visualContinuation.inputPort);
                            AddElement(edge);
                        }
                    }
                }
            }
        }

        private VisualDialogueNode DrawNode(DialogueNode dialogueNode, Vector2 position)
        {
            var node = new VisualDialogueNode(dialogueNode, dialogue.ActorNames());
            node.SetPosition(dialogueNode.Rect());
            AddElement(node);
            node.Redraw();
            //node.OutputPortsRemoved += CleanDanglingEdges;
            return node;
        }


        //ADDING REMOVING NODES

        private void CreateNode<T>(Vector2 position) where T : DialogueNode
        {
            if (dialogue == null) return;

            var node = dialogue.CreateNode<T>();
            node.SetPosition(position);
            var vNode = DrawNode(node, position);
            vNode.rootNodeToggle.RegisterValueChangedCallback((valueChangeEvent) =>
            {
                OnRootNodeToggleChanged(vNode);
            });
            FindVisualNode[node] = vNode;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (dialogue == null) return graphViewChange;

            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var elmt in graphViewChange.elementsToRemove)
                {
                    var node = elmt as VisualDialogueNode;
                    if (node != null)
                    {
                        if(node.dialogueNode == dialogue.RootNode())
                        {
                            dialogue.SetRootNode(null);
                        }
                        dialogue.DeleteNode(node.dialogueNode);
                        FindVisualNode.Remove(node.dialogueNode);
                    }

                    var edge = elmt as Edge;
                    if (edge != null)
                    {
                        var parent = edge.output?.node as VisualDialogueNode;
                        var child = edge.input?.node as VisualDialogueNode;
                        if (parent != null && child != null)
                        {
                            int responseIndex = parent.OutputPorts.IndexOf(edge.output);
                            dialogue.RemoveChild(parent.dialogueNode, child.dialogueNode, responseIndex);
                        }
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    VisualDialogueNode parent = edge.output.node as VisualDialogueNode;
                    VisualDialogueNode child = edge.input.node as VisualDialogueNode;
                    int responseIndex = parent.OutputPorts.IndexOf(edge.output);
                    dialogue.SetContinuation(parent.dialogueNode, child.dialogueNode, responseIndex);
                }
            }

            if(graphViewChange.movedElements != null)
            {
                foreach (var elmt in graphViewChange.movedElements)
                {
                    VisualDialogueNode node = elmt as VisualDialogueNode;
                    if(node != null)
                    {
                        node.dialogueNode.SetPosition(node.GetPosition().position);
                    }
                }
            }

            return graphViewChange;
        }


        //SETUP

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ListViewSelector());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent menuEvent)
        {
            base.BuildContextualMenu(menuEvent);
            if (dialogue == null) return;

            Vector2 localMousePos = viewTransform.matrix.inverse.MultiplyPoint(menuEvent.localMousePosition);

            menuEvent.menu.AppendAction("Add Auto Continuation Dialogue Node", actionEvent =>
                CreateNode<AutoContinuationDialogueNode>(localMousePos));
            menuEvent.menu.AppendAction("Add Response Choices Dialogue Node", actionEvent =>
                CreateNode<ResponseChoicesDialogueNode>(localMousePos));
            menuEvent.menu.AppendAction("Add Decision Dialogue Node", actionEvent =>
                CreateNode<DecisionDialogueNode>(localMousePos));
        }

        private void AddGridBackground()
        {
            GridBackground bkgd = new GridBackground();
            bkgd.StretchToParentSize();
            Insert(0, bkgd);
        }

        private void AddStyles()
        {
            StyleSheet styleSheet = EditorGUIUtility.Load("Dialogue/GridBackgroundStyle.uss") as StyleSheet;
            styleSheets.Add(styleSheet);
        }
    }
}
