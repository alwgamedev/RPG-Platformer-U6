using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.Dialogue
{
    public abstract class DialogueNode : ScriptableObject
    {
        [SerializeField] protected int conversantNumber;
        //[SerializeField] protected bool isPlayerSpeaking;
        [SerializeField] protected List<string> textSegments = new();
        [SerializeField] protected string uniqueID;
        [SerializeField] protected Vector2 position;//store position in editor window

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

        public abstract string ContinuationID(int responseIndex);

        public int ConversantNumber()
        {
            return conversantNumber;
        }

        //public bool IsPlayerSpeaking()
        //{
        //    return isPlayerSpeaking;
        //}

        public List<string> TextSegments()
        {
            return textSegments;
        }

        public Rect Rect()
        {
            return new(position.x, position.y, 0, 0);
        }

#if UNITY_EDITOR
        //public void SetIsPlayerSpeaking(bool val)
        //{
        //    isPlayerSpeaking = val;
        //    EditorUtility.SetDirty(this);
        //}

        public void SetConversantNumber(int val)
        {
            conversantNumber = val;
            EditorUtility.SetDirty(this);
        }

        public abstract void SetContinuation(DialogueNode node, int responseIndex);

        public abstract void RemoveContinuation(int responseIndex);

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
