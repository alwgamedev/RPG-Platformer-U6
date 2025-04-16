using RPGPlatformer.Core;
using RPGPlatformer.Saving;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    //this could also hold data like the UI Camera, if you want to make UI canvas persistent in the future
    //(or ui canvas could just "SceneManager.onSceneLoaded" find the ui camera)
    public class PlayerSpawnManager : MonoBehaviour, ISavable
    {
        [SerializeField] bool automaticallyRespawnPlayerOnDeath = true;
        [SerializeField] PlayerSpawnPoint defaultSpawnPoint;
        [SerializeField] PlayerSpawnPoint[] sceneSpawnPoints;

        public string LastPlayerCheckpoint { get; set; }//this will be the savable state

        Dictionary<string, PlayerSpawnPoint> SpawnPointLookup = new();

        private void Awake()
        {
            BuildSpawnPointLookup();
            if (automaticallyRespawnPlayerOnDeath)
            {
                GlobalGameTools.PlayerDeathFinalized += RespawnPlayerOnDeath;
            }

            SaveCheckpoint.CheckpointReached += OnSaveCheckpointReached;
        }

        private void Start()
        {
            InitialPlayerSpawn();
        }

        private async void OnSaveCheckpointReached(SaveCheckpoint checkpoint)
        {
            var id = checkpoint.gameObject.name;
            SpawnPointLookup[id] = checkpoint;
            LastPlayerCheckpoint = id;
            await SavingSystem.Instance.Save();
        }

        private void RespawnPlayerOnDeath()
        {
            SpawnPlayerToLastCheckpointOrDefault();
            GlobalGameTools.Instance.Player.Combatant.Revive();
        }

        //a scene transition trigger will take priority over last saved player checkpoint
        //(but at the same time, SceneTransitionHelper can trigger a scene transition
        //with null playerSpawnPoint id if you want to automatically spawn to last checkpoint
        //...wait but for that we need to make sure that saved state is restored before Start executes
        //...YES save system restores state in SceneManager.sceneLoaded and unity order of execution docs
        //say that onSceneLoaded will trigger after OnEnable for all objects but before any Starts, so we're good)
        private void InitialPlayerSpawn()
        {
            var l = SceneTransitionHelper.LastSceneTransition;
            if (!l.HasValue || !TrySpawnPlayer(l.Value.nextSceneData.PlayerSpawnPoint))
            {
                SpawnPlayerToLastCheckpointOrDefault();
            }
        }

        private void SpawnPlayer(PlayerSpawnPoint playerSpawnPoint)
        {
            GlobalGameTools.Instance.PlayerTransform.position = playerSpawnPoint.transform.position;
            LastPlayerCheckpoint = playerSpawnPoint.gameObject.name;
        }

        private void SpawnPlayerToLastCheckpointOrDefault()
        {
            if (!TrySpawnPlayer(LastPlayerCheckpoint))
            {
                SpawnPlayer(defaultSpawnPoint);
            }
        }

        private bool TrySpawnPlayer(string spawnPointID)
        {
            if (spawnPointID == null)
            {
                return false;
            }

            if (SpawnPointLookup.TryGetValue(spawnPointID, out var s) && s)
            {
                SpawnPlayer(s);
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
                    SpawnPointLookup[s.gameObject.name] = s;
                }
            }
        }

        public JsonNode CaptureState()
        {
            return JsonSerializer.SerializeToNode(LastPlayerCheckpoint);
        }

        public void RestoreState(JsonNode jNode)
        {
            LastPlayerCheckpoint = jNode.Deserialize<string>();
        }

        private void OnDestroy()
        {
            GlobalGameTools.PlayerDeathFinalized -= RespawnPlayerOnDeath;
            SaveCheckpoint.CheckpointReached -= OnSaveCheckpointReached;
        }
    }
}