using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public abstract class DialogueNode : ScriptableObject
    {
        [SerializeField] protected int speakerIndex;
        [SerializeField] protected List<string> textSegments = new();
        [SerializeField] protected string uniqueID;
        [SerializeField] protected Vector2 position;//store position in editor window
        [SerializeField] protected List<DialogueActionData> entryActions;
        [SerializeField] protected List<DialogueActionData> exitActions;

        public virtual void Initialize(Vector2 position = default)
        {
            uniqueID = Guid.NewGuid().ToString();
            name = $"({GetType().Name}) {uniqueID}";
            this.position = position;
        }

        public string UniqueID()
        {
            return uniqueID;
        }

        public abstract string ContinuationID(int index);

        public int SpeakerIndex()
        {
            return speakerIndex;
        }

        public List<string> TextSegments()
        {
            return textSegments;
        }

        public Rect Rect()
        {
            return new(position.x, position.y, 0, 0);
        }

        public List<DialogueActionData> EntryActions()
        {
            return entryActions;
        }

        public List<DialogueActionData> ExitActions()
        {
            return exitActions;
        }

#if UNITY_EDITOR
         public void SetSpeakerIndex(int val)
        {
            speakerIndex = val;
            EditorUtility.SetDirty(this);
        }

        public abstract void SetContinuation(DialogueNode node, int index);

        public abstract void RemoveContinuation(int index);

        public abstract void EraseAnyOccurrencesOfChild(DialogueNode child);

        public void SetTextSegment(int index, string text)
        {
            if (index < 0 || index >= textSegments.Count) return;
            textSegments[index] = text;
            EditorUtility.SetDirty(this);
        }

        public void SetPosition(Vector2 position)
        {
            this.position = position;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
