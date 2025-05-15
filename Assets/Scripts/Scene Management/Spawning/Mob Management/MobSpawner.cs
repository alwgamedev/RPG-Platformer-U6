using UnityEngine;
using RPGPlatformer.Saving;
using System.Text.Json.Nodes;

namespace RPGPlatformer.SceneManagement
{
    public class MobSpawner : MonoBehaviour, ISavable
    {
        public JsonNode CaptureState()
        {
            throw new System.NotImplementedException();
        }

        public void RestoreState(JsonNode jNode)
        {
            throw new System.NotImplementedException();
        }
    }
}