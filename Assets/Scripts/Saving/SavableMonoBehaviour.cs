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