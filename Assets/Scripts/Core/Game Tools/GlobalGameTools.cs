using RPGPlatformer.Combat;
using RPGPlatformer.Movement;
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
        IMovementController playerMover;
        ICombatController playerCC;

        public static GlobalGameTools Instance { get; private set; }
        public static string PlayerName { get; private set; } = "Player";
        public Transform PlayerTransform => playerTransform;
        //{
        //    //this is in case something needs player before GGT has done Awake, I guess?
        //    //that's terrible because FindPlayer includes subscribing to an event??
        //    get
        //    {
        //        if (playerTransform == null)
        //        {
        //            FindPlayer();
        //        }

        //        return playerTransform;
        //    }
        //}
        public IMovementController PlayerMover => playerMover;
        public ICombatController Player => playerCC;
        //{
        //    get
        //    {
        //        if (playerCC == null)
        //        {
        //            FindPlayer();
        //        }

        //        return playerCC;
        //    }
        //}
        public bool PlayerIsDead => Player == null || Player.Combatant.Health.IsDead;
        //public static bool PlayerIsInCombat => Player != null && Player.IsInCombat;
        public CancellationTokenSource TokenSource {  get; private set; }
        public TickTimer TickTimer { get; private set; }
        public ResourcesManager ResourcesManager => resourcesManager;
        public ObjectPoolCollection ProjectilePooler => projectilePooler;
        public ObjectPoolCollection EffectPooler => effectPooler;

        public static event Action PlayerDeath;//executes at the instant death state is entered
        public static event Action PlayerDeathFinalized;//executes after any death animation and delay time
        //useful to have these go through global game tools,
        //so that subscribers don't have to worry about finding player
        //or whether player even exists when subscribing

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
            resourcesManager.InitializeResources();
            FindPlayer();
        }

        private void FindPlayer()
        {
            var playerGO = GameObject.FindWithTag("Player");
            playerTransform = playerGO.transform;
            playerMover = playerTransform.GetComponent<IMovementController>();
            playerCC = playerGO.GetComponent<ICombatController>();
            playerCC.OnDeath += BroadcastPlayerDeath;
            playerGO.GetComponent<ICombatant>().DeathFinalized += BroadcastPlayerDeathFinalized;
            //***get comp instead of playerCC.Combatant, because the playerCC awake may come after GGT's,
            //which means the playerCC won't have found its combatant yet

            //var playerGO = GameObject.FindWithTag("Player");

            //if (playerGO)
            //{
            //    playerCC = playerGO.GetComponent<ICombatController>();
            //    playerTransform = playerGO.transform;
            //    if (playerCC != null)
            //    {
            //        playerCC.OnDeath += BroadcastPlayerDeath;
            //        //playerCC.Combatant.DeathFinalized += BroadcastPlayerDeathFinalized;
            //        playerGO.GetComponent<ICombatant>().DeathFinalized += BroadcastPlayerDeathFinalized;
            //        //^get comp because the playerCC awake may come after GGT's, which means the playerCC
            //        //won't have found its combatant yet
            //    }
            //}
        }

        private void BroadcastPlayerDeath()
        {
            PlayerDeath?.Invoke();
        }

        private void BroadcastPlayerDeathFinalized()
        {
            PlayerDeathFinalized?.Invoke();
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

                if (playerCC != null)
                {
                    playerCC.OnDeath -= BroadcastPlayerDeath;
                    playerCC.Combatant.DeathFinalized -= BroadcastPlayerDeathFinalized;
                }

                PlayerDeath = null;
                PlayerDeathFinalized = null;
            }
        }
    }
}