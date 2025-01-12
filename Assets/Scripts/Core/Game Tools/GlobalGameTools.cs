using RPGPlatformer.Combat;
using RPGPlatformer.SceneManagement;
using System;
using System.Threading;
using UnityEngine;

namespace RPGPlatformer.Core
{
    [RequireComponent(typeof(TickTimer))]
    public class GlobalGameTools : MonoBehaviour
    {
        [SerializeField] ResourcesManager resourcesManager = new();

        public static GlobalGameTools Instance { get; private set; }
        public static bool PlayerIsDead { get; private set; }
        public static string PlayerName { get; private set; } = "Player";

        public CancellationTokenSource TokenSource {  get; private set; }
        public TickTimer TickTimer { get; private set; }
        public ResourcesManager ResourcesManager => resourcesManager;
        public ObjectPoolCollection ProjectilePooler {  get; private set; }
        public ObjectPoolCollection EffectPooler { get; private set; }

        public static event Action OnPlayerDeath;

        public static event Action InstanceReady;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                Configure();
                InstanceReady?.Invoke();
            }
            else
            {
                Debug.Log("GlobalGameTools already has an Instance set.");
                Destroy(gameObject);
            }
        }

        private void Configure()
        {
            TokenSource = new();

            TickTimer = GetComponent<TickTimer>();

            ProjectilePooler = GameObject.Find("Projectile Pooler").GetComponent<ObjectPoolCollection>();
            EffectPooler = GameObject.Find("Effect Pooler").GetComponent<ObjectPoolCollection>();

            resourcesManager.InitializeResources();

            PlayerCombatController player = FindAnyObjectByType<PlayerCombatController>();
            player.OnDeath += () =>
            {
                PlayerIsDead = true;
                OnPlayerDeath?.Invoke();
            };
            player.OnRevive += () => PlayerIsDead = false;
        }


        private void OnDestroy()
        {
            if (!TokenSource.IsCancellationRequested)
            {
                TokenSource.Cancel();
                TokenSource.Dispose();
            }

            if (Instance == this)
            {
                Instance = null;
                InstanceReady = null;
                OnPlayerDeath = null;
            }
        }
    }
}