using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    [Serializable]
    public struct SceneTransitionTriggerData
    {
        [SerializeField] string sceneToLoad;
        [SerializeField] string playerSpawnPoint;//use null if you want to use the default spawn point

        //in the future if you need more data than just player spawn point you can make a struct
        //for "scene start data"

        public string SceneToLoad => sceneToLoad;
        public string PlayerSpawnPoint => playerSpawnPoint;

        public SceneTransitionTriggerData(string sceneToLoad, string playerSpawnPoint)
        {
            this.sceneToLoad = sceneToLoad;
            this.playerSpawnPoint = playerSpawnPoint;
        }
    }
}