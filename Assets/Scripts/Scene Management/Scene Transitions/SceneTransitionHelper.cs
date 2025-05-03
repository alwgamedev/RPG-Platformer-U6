using RPGPlatformer.Saving;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGPlatformer.SceneManagement
{
    public class SceneTransitionHelper : MonoBehaviour, ISavable
    {
        public string LastGameLevelPlayed { get; private set; }
        //the savable state
        //separate from "LastSceneTransition" which will be reset every time the game is opened
        //and includes all scene transitions (including to/from start menu)

        public static SceneTransitionData? LastSceneTransition { get; private set; } = null;

        public static SceneTransitionHelper Instance;        

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                ISceneTransitionTrigger.SceneTransitionTriggered += LoadScene;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public JsonNode CaptureState()
        {
            return JsonSerializer.SerializeToNode(LastGameLevelPlayed);
        }

        public void RestoreState(JsonNode jNode)
        {
            LastGameLevelPlayed = jNode.Deserialize<string>();
        }

        public async void LoadLastGameLevelPlayed()
        {
            if (LastGameLevelPlayed == null) return;

            await LoadSceneTask(new SceneTransitionTriggerData(LastGameLevelPlayed, null));
            //the player spawn manager in that level will then automatically send you to the last checkpoint
            //(or to default spawn point)
        }

        public async void LoadScene(SceneTransitionTriggerData data)
        {
            await LoadSceneTask(data);
        }

        private async Task LoadSceneTask(SceneTransitionTriggerData data)
        {
            LastSceneTransition = new SceneTransitionData(SceneManager.GetActiveScene().name, data);
            if (data.SceneToLoad != "StartMenu")//placeholder for now
            {
                LastGameLevelPlayed = data.SceneToLoad;
            }
            //and in the future we can destroy/instantiate game ui canvas when we transition to/from start menu

            await SavingSystem.Instance.Save();

            var ao = SceneManager.LoadSceneAsync(data.SceneToLoad);
            ao.allowSceneActivation = false;

            while (!ao.isDone)
            {
                if (ao.progress >= 0.9f)
                {
                    Debug.Log("INITIATING LAG!");
                    ao.allowSceneActivation = true;
                    break;
                }

                await Task.Yield();
            }

            //saving system will then restore state in response to "SceneManager.sceneLoaded" event
            //(do it that way, because not every scene loading may go through the SceneTransitionHelper,
            //e.g. the first scene load upon opening the game;
            //it's also more reliable -- Unity order of execution guarantees that sceneLoaded fires
            //after OnEnable for all objects but BEFORE START, so we can be sure to have restored state
            //before any Starts
            //--EDIT: NO we can't guarantee state is restored before start bc loading is async and takes time...
            //(we had this issue with PlayerSpawnManager and Combatant))
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                LastSceneTransition = null;
                ISceneTransitionTrigger.SceneTransitionTriggered -= LoadScene;
            }
        }
    }
}