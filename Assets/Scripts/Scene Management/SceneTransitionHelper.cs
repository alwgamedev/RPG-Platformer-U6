using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGPlatformer.SceneManagement
{
    public class SceneTransitionHelper : MonoBehaviour
    {
        bool configured;

        public static SceneTransitionData? LastSceneTransition { get; private set; } = null;

        public static SceneTransitionHelper Instance;        

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Configure();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        //for now just doing async so we don't block and have a smoother transition
        //(but we don't need to do anything at end of await; we'll just store whatever args we need
        //and classes that need them can get them from the Instance)
        //(we can also use the (built in) unity events SceneManager.sceneLoaded and sceneUnloaded)
        public async void LoadScene(SceneTransitionTriggerData data)
        {
            LastSceneTransition = new SceneTransitionData(SceneManager.GetActiveScene().name,
                data.SceneToLoad, data.SceneStarterData);
            await SceneManager.LoadSceneAsync(data.SceneToLoad);
        }
        //BTW this is a nice way of doing it, because from start menu we could have saving system load 
        //the last player checkpoint data (or w/e) and load scene with the appropriate args;
        //we could even load the checkpoint data into a scene trigger component attached to a "Play Game" button
        //or something like that

        private void Configure()
        {
            if (configured) return;

            configured = true;
            ISceneTransitionTrigger.SceneTransitionTriggered += LoadScene;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                LastSceneTransition = null;
            }

            ISceneTransitionTrigger.SceneTransitionTriggered -= LoadScene;
        }
    }
}