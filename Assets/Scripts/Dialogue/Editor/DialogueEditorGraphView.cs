using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System;

namespace RPGPlatformer.Dialogue.Editor
{
    public class DialogueEditorGraphView : GraphView
    {
        public DialogueEditorGraphView()
        {
            AddManipulators();
            AddGridBackground();
            AddStyles();
        }

        private DialogueNode CreateNode<T>(Vector2 position) where T : DialogueNode
        {
            DialogueNode node = (T)Activator.CreateInstance(typeof(T));
            node.Draw();
            node.SetPosition(new Rect(position, default));
            return node;
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateNodeContextualMenu());
        }

        private IManipulator CreateNodeContextualMenu()
        {
            //Create options in right click menu for adding new nodes
            ContextualMenuManipulator contextualMenuManipulator = 
                new(menuEvent =>
                {
                    menuEvent.menu.AppendAction("Add Dialogue Node With Auto Continuation",
                        actionEvent =>
                        AddElement(CreateNode<DialogueNodeWithAutoContinuation>(actionEvent.eventInfo.localMousePosition)));
                    menuEvent.menu.AppendAction("Add Dialogue Node With Response Choices",
                        actionEvent =>
                        AddElement(CreateNode<DialogueNodeWithResponseChoices>(actionEvent.eventInfo.localMousePosition)));
                });
            return contextualMenuManipulator;
        }

        private void AddGridBackground()
        {
            GridBackground bkgd = new GridBackground();
            bkgd.StretchToParentSize();
            Insert(0, bkgd);
        }

        private void AddStyles()
        {
            StyleSheet styleSheet = EditorGUIUtility.Load("Dialogue/DialogueGraphViewStyles.uss") as StyleSheet;
            styleSheets.Add(styleSheet);
        }
    }
}
