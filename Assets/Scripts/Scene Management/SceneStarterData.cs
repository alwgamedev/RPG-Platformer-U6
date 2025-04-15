using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    [Serializable]
    public struct SceneStarterData
    {
        [SerializeField] PlayerSpawnPoint playerSpawnPoint;

        public PlayerSpawnPoint PlayerSpawnPoint => playerSpawnPoint;
    }
}