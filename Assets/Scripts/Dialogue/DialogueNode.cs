using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGPlatformer.Dialogue
{
    public abstract class DialogueNode : Node//this could be SO instead (like in KiwiCoder's tutorial)
    {
        [SerializeField] protected List<string> childIDs = new();
        [SerializeField] protected int speakerIndex = -1;//-1 will always be player
        [SerializeField] protected List<string> textSegments = new();

        public List<string> ChildIDs()
        {
            return childIDs;
        }

        public int SpeakerIndex()
        {
            return speakerIndex;
        }

        public List<string> TextSegments()
        {
            return textSegments;
        }

        public void Position(Vector2 position)
        {
            SetPosition(new Rect(position, default));
        }

        public void Draw()
        {
            DrawTitleContainer();
            DrawInputContainer();
            DrawOutputContainer();
            DrawExtensionsContainer();
        }

        protected void DrawTitleContainer()
        {
            TextField title = new("Speaker number:")
            {
                value = "-1"
                //TO-DO: enforce integer input only in OnGUI (i.e. if character out of range, clear the textfield)
                //(can just check if int32.tryparse the current value)
                //(we can even have the node hold a maxSpeaker index (defaulting usefuly to 0, as most dialogue will
                //only have one ai speaker)
            };
            titleContainer.Insert(0, title);
        }

        protected void DrawInputContainer()
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input,
                Port.Capacity.Multi, typeof(Node));
            inputPort.portName = "Parent";
            inputContainer.Insert(0, inputPort);
        }

        protected abstract void DrawOutputContainer();

        protected void DrawExtensionsContainer()
        {
            Foldout dialogueFoldout = new()
            {
                text = "Speaker Dialogue"
            };

            for(int i = 0; i < textSegments.Count; i++)
                //We would also want to be able to drag the segments around and reorder...
                //(might be a bit of a pain to keep textSegments field updated)
            {
                VisualElement segmentContainer = new();
                TextField textSegment = new(textSegments[i]);
                Button deleteButton = new();
                deleteButton.text = "Delete";
                //deleteButton.clicked += () =>
                //{
                //    textSegments.RemoveAt(i);
                //    Draw();
                //};

                segmentContainer.Insert(0, textSegment);
                segmentContainer.Insert(1, deleteButton);

                dialogueFoldout.Insert(i, segmentContainer);
            }

            Button addButton = new();
            addButton.text = "Add";
            dialogueFoldout.Insert(dialogueFoldout.childCount, addButton);

            extensionContainer.Insert(0, dialogueFoldout);

            //+there should be button for new text segment

            RefreshExpandedState();
        }
    }
}
