using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace RPGPlatformer.Saving
{
    //manages unique ID of a MB we want to save
    //captures/restores state of all ISavables on the MB
    [ExecuteAlways]
    public class SavableMonoBehaviour : MonoBehaviour
    {
        [SerializeField] string uniqueIdentifier;
        
        //Dictionary<string, ISavable> ComponentLookup = new();

        public string UniqueIdentifier => uniqueIdentifier;

        public static Dictionary<string, SavableMonoBehaviour> GlobalLookup = new();

#if UNITY_EDITOR
        private void Update()
        {
            if (Application.IsPlaying(gameObject) || string.IsNullOrEmpty(gameObject.scene.path)) return;

            SerializedObject serObject = new SerializedObject(this);
            SerializedProperty serProperty = serObject.FindProperty("uniqueIdentifier");

            if (string.IsNullOrEmpty(serProperty.stringValue) || !IsUnique(serProperty.stringValue))
            {
                serProperty.stringValue = System.Guid.NewGuid().ToString();
                serObject.ApplyModifiedProperties();
                GlobalLookup[serProperty.stringValue] = this;
            }
        }
#endif

        public JsonObject CaptureState()
        {
            JsonObject componentStates = new();

            foreach(var savable in GetComponents<ISavable>())
            {
                //ISavable savable = entry.Value;
                componentStates[savable.GetType().Name] = savable.CaptureState();

                Debug.Log($"{GetType().Name} on {gameObject.name} " +
                    $"captured state of component of type {savable.GetType().Name}");
            }
            return componentStates;
        }

        //NOTE: the savable components are only distinguished by their type, so cannot accommodate multiple ISavable
        //components of the same type (most likely this won't be an issue)
        //I tried to fix this by keeping component IDs in a dictionary but serializing the dictionary (so that it stays with
        //the game object always) is a bigly pain to solve something that's most likely a non-issue
        public void RestoreState(JsonObject state)
        {
            IDictionary<string, JsonNode> stateDict = state;
            var savables = GetComponents<ISavable>();

            foreach(var savable in savables)
            {
                if (stateDict.ContainsKey(savable.GetType().Name))
                {
                    JsonNode jNode = stateDict[savable.GetType().Name];
                    savable.RestoreState(jNode);

                    Debug.Log($"{GetType().Name} on {gameObject.name} " +
                        $"restored state of component of type {savable.GetType().Name}");
                }
            }
        }

        //private void UpdateComponentLookup()
        //{
        //    SerializedObject serObject = new SerializedObject(this);
        //    SerializedProperty serProperty = serObject.FindProperty("ComponentLookup");

        //    var savableComponents = GetComponents<ISavable>();
        //    var lookup = (Dictionary<string, ISavable>)serProperty;

        //    foreach(var entry in serProperty)
        //    {
        //        //Remove deleted components from the Lookup
        //        if (!savableComponents.Contains(entry.Value))
        //        {
        //            ComponentLookup.Remove(entry.Key);
        //        }
        //    }

        //    foreach(var savable in savableComponents)
        //    {
        //        //Add any new components to the Lookup
        //        if (!ComponentLookup.ContainsValue(savable))
        //        {
        //            string compID = System.Guid.NewGuid().ToString();
        //            ComponentLookup[compID] = savable;
        //        }
        //    }
        //}

        private bool IsUnique(string candidate)
        {
            if (!GlobalLookup.ContainsKey(candidate)) return true;

            if (GlobalLookup[candidate] == this) return true;

            if (GlobalLookup[candidate] == null)
            {
                GlobalLookup.Remove(candidate);
                return true;
            }

            if (GlobalLookup[candidate].UniqueIdentifier != candidate)
            {
                GlobalLookup.Remove(candidate);
                return true;
            }

            return false;
        }

    }
}