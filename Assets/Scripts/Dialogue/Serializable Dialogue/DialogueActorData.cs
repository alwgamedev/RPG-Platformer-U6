using UnityEngine;
using System;
using System.Collections.Generic;
using RPGPlatformer.Core;

namespace RPGPlatformer.Dialogue
{
    [Serializable]
    public struct DialogueActorData
    {
        [SerializeField] LabelledUnityObject<DialogueActor>[] actors;
        //this allows us to
        //a) use the correct actor name labels in the inspector & maintain the correct array size
        //b) allow an actor name to be used in a dialogue, even if there is no DialogueActor mb assigned
            //(so the actor does not have to be present in the scene)

        Dictionary<int, LabelledUnityObject<DialogueActor>> Actors;

        public LabelledUnityObject<DialogueActor> this[int i]
        {
            get => actors[i];
        }

        public IEnumerable<DialogueActor> GetActors()
        {
            foreach (var actor in actors)
            {
                yield return actor.labelledObject;
            }
        }

        public void OnBeforeSerialize(DialogueSO dialogue)
        {
            if (dialogue == null || dialogue.ActorNames() == null)
            {
                actors = new LabelledUnityObject<DialogueActor>[0];
                return;
            }

            var names = dialogue.ActorNames();

            actors = new LabelledUnityObject<DialogueActor>[names.Count];

            for (int i = 0; i < actors.Length; i++)
            {
                if (Actors.TryGetValue(i, out var c))
                {
                    Actors[i] = new(names[i], Actors[i].labelledObject);
                }
                else
                {
                    Actors[i] = new LabelledUnityObject<DialogueActor>(names[i], null);
                }

                actors[i] = Actors[i];
            }
        }

        public void OnAfterDeserialize()
        {
            Actors = new();

            if (actors == null) return;

            for (int i = 0; i < actors.Length; i++)
            {
                Actors[i] = actors[i];
            }
        }
    }
}
