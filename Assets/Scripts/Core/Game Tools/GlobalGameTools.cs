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
        [SerializeField] ObjectPoolCollection projectilePooler;//should be children of the GGT
        [SerializeField] ObjectPoolCollection effectPooler;

        Transform playerTransform;
        ICombatController playerCC;

        public static GlobalGameTools Instance { get; private set; }
        public static string PlayerName { get; private set; } = "Player";
        public Transform PlayerTransform
        {
            get
            {
                if (playerTransform == null)
                {
                    FindPlayer();
                }

                return playerTransform;
            }
        }
        public ICombatController Player
        {
            get
            {
                if (playerCC == null)
                {
                    FindPlayer();
                }

                return playerCC;
            }
        }
        public bool PlayerIsDead => Player == null || Player.Combatant.Health.IsDead;
        //public static bool PlayerIsInCombat => Player != null && Player.IsInCombat;
        public CancellationTokenSource TokenSource {  get; private set; }
        public TickTimer TickTimer { get; private set; }
        public ResourcesManager ResourcesManager => resourcesManager;
        public ObjectPoolCollection ProjectilePooler => projectilePooler;
        public ObjectPoolCollection EffectPooler => effectPooler;

        public static event Action OnPlayerDeath;
        //useful to have this go through global game tools,
        //so that subscribers don't have to worry about whether player exists when subscribing
        //(i.e. compared to directly subscribing to player)

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

            //ProjectilePooler = GameObject.Find("Projectile Pooler").GetComponent<ObjectPoolCollection>();
            //EffectPooler = GameObject.Find("Effect Pooler").GetComponent<ObjectPoolCollection>();

            resourcesManager.InitializeResources();

            FindPlayer();
        }

        private void FindPlayer()
        {
            var playerGO = GameObject.FindWithTag("Player");

            if (playerGO)
            {
                playerCC = playerGO.GetComponent<PlayerCombatController>();
                playerTransform = playerGO.transform;
                if (playerCC != null)
                {
                    playerCC.OnDeath += () => OnPlayerDeath?.Invoke();
                }
            }
        }


        private void OnDestroy()
        {
            if (TokenSource != null)
            {
                if (!TokenSource.IsCancellationRequested)
                {
                    TokenSource.Cancel();
                }
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