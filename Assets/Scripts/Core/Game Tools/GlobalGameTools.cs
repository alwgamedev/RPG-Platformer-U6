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
        public static GlobalGameTools Instance { get; private set; }
        public static bool PlayerIsDead { get; private set; }

        public CancellationTokenSource TokenSource {  get; private set; }
        public TickTimer TickTimer { get; private set; }
        public ResourcesManager ResourcesManager { get; private set; }
        public ObjectPoolCollection ProjectilePooler {  get; private set; }
        public ObjectPoolCollection EffectPooler { get; private set; }

        public event Action OnPlayerDeath;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                Configure();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Configure()
        {
            TokenSource = new();

            TickTimer = GetComponent<TickTimer>();

            ProjectilePooler = GameObject.Find("Projectile Pooler").GetComponent<ObjectPoolCollection>();
            EffectPooler = GameObject.Find("Effect Pooler").GetComponent<ObjectPoolCollection>();

            ResourcesManager = new();
            ResourcesManager.InitializeResources();

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

            OnPlayerDeath = null;
        }
    }
}