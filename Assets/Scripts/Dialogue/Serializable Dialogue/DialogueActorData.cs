using UnityEngine;
using System;
using System.Collections.Generic;
using RPGPlatformer.Core;
using System.Linq;

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

        //to be called in Start by a relevant monobehavior
        public void FindPlayer()
        {
            for (int i = 0; i < actors.Length; i++)
            {
                if (actors[i].label == "Player")
                {
                    var a = actors[i];
                    a.labelledObject = GlobalGameTools.Instance.PlayerTransform.GetComponent<DialogueActor>();
                    actors[i] = a;
                    Actors[i] = a;
                    //also update dict, just because OnBeforeSerialize was getting called if you open dialogue trigger's
                    //inspector during play mode and overwriting actors array
                }
            }
        }

        public LabelledUnityObject<DialogueActor> this[int i] => actors[i];

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
