using RPGPlatformer.Cinematics;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using RPGPlatformer.Saving;
using RPGPlatformer.UI;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using UnityEngine;

namespace RPGPlatformer.SceneManagement
{
    public class PlayerSpawnManager : MonoBehaviour, ISavable
    {
        [SerializeField] bool useDefaultOverSaveData;//mainly for testing so I can spawn wherever I want if needed
        [SerializeField] bool automaticallyRespawnPlayerOnDeath = true;
        [SerializeField] PlayerSpawnPoint defaultSpawnPoint;
        [SerializeField] PlayerSpawnPoint[] sceneSpawnPoints;

        float playerHeight;
        string lastPlayerCheckpoint;//this will be the savable state

        Dictionary<string, PlayerSpawnPoint> SpawnPointLookup = new();

        private void Awake()
        {
            BuildSpawnPointLookup();
            if (automaticallyRespawnPlayerOnDeath)
            {
                GlobalGameTools.PlayerDeathFinalized += RespawnPlayerOnDeath;
            }

            SaveCheckpoint.CheckpointReached += OnSaveCheckpointReached;
            SavingSystem.SceneLoadComplete += InitialPlayerSpawn;
        }

        private async void OnSaveCheckpointReached(SaveCheckpoint checkpoint)
        {
            var id = checkpoint.gameObject.name;
            SpawnPointLookup[id] = checkpoint;
            lastPlayerCheckpoint = id;
            await SavingSystem.Instance.Save();
        }

        private void RespawnPlayerOnDeath()
        {
            SpawnPlayerToLastCheckpointOrDefault();
            GlobalGameTools.Instance.Player.Combatant.Revive();
            GameLog.Log("(You have been revived at the last checkpoint.)");
        }

        //a scene transition trigger will take priority over last saved player checkpoint
        //(but at the same time, SceneTransitionHelper can trigger a scene transition
        //with null playerSpawnPoint id if you want to automatically spawn to last checkpoint
        //...wait but for that we need to make sure that saved state is restored before Start executes
        //...YES save system restores state in SceneManager.sceneLoaded and unity order of execution docs
        //say that onSceneLoaded will trigger after OnEnable for all objects but before any Starts, so we're good)
        private void InitialPlayerSpawn()
        {
            playerHeight = GlobalGameTools.Instance.PlayerTransform.GetComponent<IMover>().Height;

            var l = SceneTransitionHelper.LastSceneTransition;
            if (!l.HasValue || !TrySpawnPlayer(l.Value.nextSceneData.PlayerSpawnPoint))
            {
                SpawnPlayerToLastCheckpointOrDefault();
            }

            SavingSystem.SceneLoadComplete -= InitialPlayerSpawn;
        }

        private void SpawnPlayer(PlayerSpawnPoint playerSpawnPoint)
        {
            PlayerFollowCamera.FollowPlayer(false);

            GlobalGameTools.Instance.PlayerTransform.position = 
                playerSpawnPoint.transform.position + 0.55f * playerHeight * Vector3.up;
            lastPlayerCheckpoint = playerSpawnPoint.gameObject.name;

            PlayerFollowCamera.FollowPlayer(true);
            //so that the lookahead doesn't cause a wobble when we first start the scene
        }

        private void SpawnPlayerToLastCheckpointOrDefault()
        {
            if (useDefaultOverSaveData || !TrySpawnPlayer(lastPlayerCheckpoint))
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
            return JsonSerializer.SerializeToNode(lastPlayerCheckpoint);
        }

        public void RestoreState(JsonNode jNode)
        {
            lastPlayerCheckpoint = jNode.Deserialize<string>();
        }

        private void OnDestroy()
        {
            SavingSystem.SceneLoadComplete -= InitialPlayerSpawn;
            GlobalGameTools.PlayerDeathFinalized -= RespawnPlayerOnDeath;
            SaveCheckpoint.CheckpointReached -= OnSaveCheckpointReached;
        }
    }
}