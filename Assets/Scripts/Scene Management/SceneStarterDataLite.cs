using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    [Serializable]
    public struct SceneStarterDataLite
    {
        [SerializeField] string playerSpawnPoint;

        public string PlayerSpawnPoint => playerSpawnPoint;

        //for now it's silly, but in the future we may need more data
    }
}