using RPGPlatformer.Core;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    //this could also hold data like the UI Camera, if you want to make UI canvas persistent in the future
    //(or ui canvas could just "SceneManager.onSceneLoaded" find the ui camera)
    public class SceneConfigurer : MonoBehaviour
    {
        [SerializeField] SceneStarterData defaultData;
        [SerializeField] PlayerSpawnPoint[] sceneSpawnPoints;

        Dictionary<string, PlayerSpawnPoint> SpawnPointLookup = new();

        private void Awake()
        {
            BuildSpawnPointLookup();
        }

        private void Start()
        {
            ConfigureScene();
        }

        //all pretty dumb for now, but in the future we will have more data than just player spawn point
        //(or if we don't then we'll simplify this)
        private void ConfigureScene()
        {
            var l = SceneTransitionHelper.LastSceneTransition;
            if (!l.HasValue || !TryConfigureScene(l.Value.sceneStarterData))
            {
                ConfigureScene(defaultData);
            }
        }

        private void ConfigureScene(SceneStarterData data)
        {
            SetPlayerPosition(data.PlayerSpawnPoint);
        }

        private bool TryConfigureScene(SceneStarterDataLite data)
        {
            return TrySetPlayerPosition(data.PlayerSpawnPoint);
        }

        private void SetPlayerPosition(PlayerSpawnPoint playerSpawnPoint)
        {
            GlobalGameTools.Instance.PlayerTransform.position = playerSpawnPoint.transform.position;
        }

        private bool TrySetPlayerPosition(string spawnPointID)
        {
            if (spawnPointID == null)
            {
                return false;
            }

            if (SpawnPointLookup.TryGetValue(spawnPointID, out var s) && s)
            {
                GlobalGameTools.Instance.PlayerTransform.position = s.transform.position;
                return true;
            }

            return false;
        }

        private void BuildSpawnPointLookup()
        {
            SpawnPointLookup.Clear();

            if (sceneSpawnPoints != null)
            {
                foreach (var s in sceneSpawnPoints)
                {
                    SpawnPointLookup[s.ID] = s;
                }
            }
        }
    }
}