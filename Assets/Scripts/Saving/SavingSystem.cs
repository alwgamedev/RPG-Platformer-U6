using System.Text.Json.Nodes;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGPlatformer.Saving
{
    using static SavingUtilities;

    public class SavingSystem : MonoBehaviour
    {
        public static SavingSystem Instance;

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
        }

        public async Task Save()
        {
            await Save(DefaultFilePath());
        }

        public async Task Load()
        {
            await Load(DefaultFilePath());
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
            }

            //SettingsManager.IAMConfigured -= OnIAMConfigure;
        }
    }
}
