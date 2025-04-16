using System;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class SaveCheckpoint : PlayerSpawnPoint
    {   
        public static event Action<SaveCheckpoint> CheckpointReached;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy) return;

            if (collider.gameObject.CompareTag("Player"))
            {
                CheckpointReached?.Invoke(this);
            }
        }
    }
}