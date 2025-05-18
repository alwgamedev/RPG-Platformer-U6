using System.Text.Json.Nodes;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using RPGPlatformer.Core;
using System;

namespace RPGPlatformer.Saving
{
    using static SavingUtilities;

    public class SavingSystem : MonoBehaviour
    {
        public static SavingSystem Instance;

        public static event Action SceneLoadComplete;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                SceneManager.sceneLoaded += OnSceneLoaded;
                //SettingsManager.IAMConfigured += OnIAMConfigure;
            }
            else
            {
                Destroy(gameObject);
            }
            //in case we have a saving system that persisted from the start menu or something
        }

        private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            await Load();
            SceneLoadComplete?.Invoke();
        }

        public async Task Save()
        {
            await Save(DefaultFilePath(), GlobalGameTools.Instance.TokenSource.Token);
            Debug.Log($"{gameObject.name}: save complete");
        }

        public async Task Load()
        {
            await Load(DefaultFilePath(), GlobalGameTools.Instance.TokenSource.Token);
            Debug.Log($"{gameObject.name}: load complete");
        }

        public async Task Save(string filePath, CancellationToken token)
        {
            Debug.Log($"Saving to {filePath}");

            JsonObject save = await LoadJObjectFromFile(filePath, token);
            CaptureSceneToJObject(save);
            await SaveJObjectToFile(save, filePath, token);
        }

        public async Task Load(string filePath, CancellationToken token)
        {
            Debug.Log($"Loading from {filePath}");

            JsonObject save = await LoadJObjectFromFile(filePath, token);
            RestoreSceneFromJObject(save);
        }

        private void CaptureSceneToJObject(JsonObject jObject)
        {
            if(jObject == null)
            {
                Debug.Log("jobject null");
                return;
            }

            foreach(var savable in 
                FindObjectsByType<SavableMonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
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
            return Path.Combine(Application.persistentDataPath, "LunchtimeRPGSave.json");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneLoadComplete = null;
            }

            //SettingsManager.IAMConfigured -= OnIAMConfigure;
        }
    }
}
