using System.Collections.Generic;
//using System.Json;
using System.Text.Json.Nodes;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Saving
{
    using static SavingUtilities;

    public class SavingSystem : MonoBehaviour
    {
        private void OnEnable()
        {
            SettingsManager.OnIAMConfigure += OnIAMConfigure;
        }

        private void OnIAMConfigure()
        {
            SettingsManager.Instance.IAM.SaveAction.canceled += async (context) => await Save(DefaultFilePath());
            SettingsManager.Instance.IAM.LoadAction.canceled += async (context) => await Load(DefaultFilePath());

            //SettingsManager.OnIAMConfigure -= OnIAMConfigure;
        }

        public async Task Save(string filePath)
        {
            Debug.Log($"Saving to {filePath}");

            JsonObject save = await LoadJObjectFromFile(filePath);
            CaptureSceneToJObject(save);
            await SaveJObjectToFile(save, filePath);
        }

        public async Task Load(string filePath)
        {
            Debug.Log($"Loading from {filePath}");

            JsonObject save = await LoadJObjectFromFile(filePath);
            RestoreSceneFromJObject(save);
        }

        private void CaptureSceneToJObject(JsonObject jObject)
        {
            if(jObject == null)
            {
                Debug.Log("jobject null");
                return;
            }

            foreach(var savable in FindObjectsByType<SavableMonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if(savable == null)
                {
                    Debug.Log("savable null");
                    continue;
                }
                if(savable.UniqueIdentifier == null)
                {
                    Debug.Log("uid null");
                    continue;
                }
                jObject[savable.UniqueIdentifier] = savable.CaptureState();
            }
        }

        private void RestoreSceneFromJObject(JsonObject jObject)
        {
            foreach (var savable in FindObjectsByType<SavableMonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if(jObject.ContainsKey(savable.UniqueIdentifier))
                {
                    savable.RestoreState((JsonObject)jObject[savable.UniqueIdentifier]);
                }
            }
        }

        private string DefaultFilePath()
        {
            return Path.Combine(Application.persistentDataPath, "RPGPlatformerTestSave.json");
        }
    }
}
