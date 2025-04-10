using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AICombatController))]
    [RequireComponent(typeof(EarthwormMovementController))]
    public class EarthwormDriver : StateDriver
    {
        [SerializeField] float emergeMoveSpeed = .5f;
        [SerializeField] float retreatMoveSpeed = 2;
        [SerializeField] float tunnelMoveSpeed = 8;
        [SerializeField] float maxTimeAboveGround = 10; 
        [SerializeField] Transform underGroundBodyAnchor;
        [SerializeField] Transform aboveGroundBodyAnchor;
        [SerializeField] EarthwormWormhole wormhole;

        AICombatController combatController;
        EarthwormMovementController movementController;
        VisualCurveGuide curveGuide;
        //Transform currentBodyAnchor;
        bool wormholeTriggerEnabled;

        //body anchors should have fixed local positions (i.e. not attached to guide points)
        //Transform BodyAnchor => movementController.bodyAnchor;

        PolygonCollider2D GroundCollider => movementController.GroundCollider;
        float GroundLeftBound => movementController.GroundLeftBound;//giving a little padding for safety
        float GroundRightBound => movementController.GroundRightBound;
        float GroundTopBound => movementController.GroundTopBound;
        //^higher than any point on the groundCollider (so if we take ClosestPoint
        //from a point at this height, we will always get something on the top side of the ground)
        Vector3 BodyAnchorOffset => movementController.BodyAnchorOffset;
        //Vector3 AnchoredPosition => wormholeAnchor.transform.position - BodyAnchorOffset;
        //^where transform.position should be when body anchor is connected to wormhole anchor
        //public TriggerColliderMessenger WormholeAnchor => wormhole.Trigger;
        public IHealth CurrentTarget
        {
            get => combatController.currentTarget;
            set => combatController.currentTarget = value;
        }
        public ICombatController CombatController => combatController;

        private void Awake()
        {
            combatController = GetComponent<AICombatController>();
            movementController = GetComponent<EarthwormMovementController>();
            curveGuide = GetComponentInChildren<VisualCurveGuide>();
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.P))
        //    {
        //        testAboveGround = !testAboveGround;
        //        if (testAboveGround)
        //        {
        //            Trigger(typeof(EarthwormAboveGround).Name);
        //        }
        //        else
        //        {
        //            Trigger(typeof(EarthwormDormant).Name);
        //        }
        //    }
        //}

        public void InitializeState()
        {
            CurrentTarget = GlobalGameTools.Player.Combatant.Health;

            curveGuide.ikTarget = GlobalGameTools.PlayerTransform;
            curveGuide.ikEnabled = false;

            SetBodyAnchor(false);
            ChooseRandomWormholePosition();
            GoToWormhole();
            SetAutoRetaliate(false);
            SetInvincible(true);
            Trigger(typeof(EarthwormDormant).Name);
        }


        //SETTINGS

        //think I will hook these up to animation events
        public void EnableIK()
        {
            curveGuide.ikEnabled = true;
        }

        public void DisableIK()
        {
            curveGuide.ikEnabled = false;
        }

        public void SetAutoRetaliate(bool val)
        {
            combatController.autoRetaliate = val;
        }

        public void SetInvincible(bool val)
        {
            combatController.Combatant.SetInvincible(val);
        }

        //only enabled in dormant and retreat states,
        //and he can only leave those states via the wormhole trigger.
        //wormhole being triggered automatically disables the trigger,
        //so we should never have to directly disable the trigger
        public void EnableWormholeTrigger(bool val)
        {
            if (val == wormholeTriggerEnabled) return;

            if (val)
            {
                wormholeTriggerEnabled = true;
                wormhole.Trigger.TriggerEnter += OnWormholeTriggered;
                wormhole.Trigger.TriggerStay += OnWormholeTriggered;
            }
            else
            {
                wormholeTriggerEnabled = false;
                wormhole.Trigger.TriggerEnter -= OnWormholeTriggered;
                wormhole.Trigger.TriggerStay -= OnWormholeTriggered;
            }
        }

        private void OnWormholeTriggered(Collider2D collider)
        {
            if (collider.transform == CurrentTarget.transform)
            {
                EnableWormholeTrigger(false);
                Trigger(typeof(EarthwormAboveGround).Name);
            }
        }


        //STATE BEHAVIORS

        public void AboveGroundBehavior()
        {
            if (CurrentTarget != null && !combatController.Combatant.CanAttack(CurrentTarget))
            {
                Trigger(typeof(EarthwormPursuit).Name);
                //if this triggers while worm is still emerging, will we have issues?
                //(maybe due to slow finally calls)
            }
        }

        public async Task AboveGroundTimer(CancellationToken token)
        {
            await MiscTools.DelayGameTime(maxTimeAboveGround, token);
        }

        public void AboveGroundTimerComplete()
        {
            Trigger(typeof(EarthwormRetreat).Name);
        }

        public async Task PursueTarget(CancellationToken token)
        {
            var p = new Vector2(CurrentTarget.transform.position.x, GroundTopBound);
            var q = GroundCollider.ClosestPoint(p);
            SetWormholePosition(q);
            await TunnelTowardsAnchor(token);
        }

        //making this separate so that handler of the pursuit -> aboveground transition 
        //doesn't get wrapped up in the previous x -> pursuit transition
        public void PursuitComplete()
        {
            Trigger(typeof(EarthwormAboveGround).Name);
        }


        //COMBAT

        //TO-DO: randomize attack speed & give it custom (possibly randomized) ability cycle
        //+ figure out what to do with collision during slam attack
        //(ideally disable collider but apply force to player (sending them up and backwards) and briely stun them

        public void StartAttacking()
        {
            combatController.StartAttacking();
        }

        public void StopAttacking()
        {
            combatController.StopAttacking();
        }

        public void FaceTarget()
        {
            combatController.FaceAimPosition();
        }


        //EMERGE & RETREAT

        public async Task Submerge(CancellationToken token)
        {
            SetBodyAnchor(false);
            await MoveToAnchorPosition(retreatMoveSpeed, token);
        }

        public async Task Emerge(CancellationToken token)
        {
            SetBodyAnchor(true);
            wormhole.PlayEmergeEffect();
            await MoveToAnchorPosition(emergeMoveSpeed, token);
        }


        //POSITIONING

        public void ChooseRandomWormholePosition()
        {
            ChooseRandomWormholePosition(GroundLeftBound + 0.5f, GroundRightBound - 0.5f);
        }

        public void ChooseRandomWormholePosition(float leftXBd, float rightXBd)
        {
            var x = Random.Range(leftXBd, rightXBd);
            var p = GroundCollider.ClosestPoint(new Vector2(x, GroundTopBound));
            SetWormholePosition(p);
        }

        public void SetWormholePosition(Vector3 position)
        {
            wormhole.SetPosition(position);
        }

        public void GoToWormhole()
        {
            movementController.GoTo(wormhole.transform.position - BodyAnchorOffset);
        }

        public void SetBodyAnchor(bool aboveGround)
        {
            movementController.bodyAnchor = aboveGround ? aboveGroundBodyAnchor : underGroundBodyAnchor;
        }

        //top level caller needs to handle cancellation
        public async Task MoveToAnchorPosition(float moveSpeed, CancellationToken token)
        {
            TaskCompletionSource<object> tcs = new();
            using var reg = token.Register(Cancel);

            void Complete()
            {
                tcs.TrySetResult(null);
            }

            void Cancel()
            {
                tcs.TrySetCanceled();
            }

            try
            {
                movementController.DestinationReached += Complete;
                combatController.OnDeath += Cancel;
                movementController.BeginMoveTowards(wormhole.transform.position - BodyAnchorOffset, moveSpeed);
                await tcs.Task;
            }
            finally
            {
                movementController.DestinationReached -= Complete;
                combatController.OnDeath -= Cancel;
            }
        }

        public async Task TunnelTowardsAnchor(CancellationToken token)
        {
            TaskCompletionSource<object> tcs = new();
            using var reg = token.Register(Cancel);

            void Complete()
            {
                tcs.TrySetResult(null);
            }

            void Cancel()
            {
                tcs.TrySetCanceled();
            }

            try
            {
                movementController.DestinationReached += Complete;
                combatController.OnDeath += Cancel;
                movementController.PlayTunnelingParticles();
                movementController.BeginMoveAnchored(wormhole.transform.position, tunnelMoveSpeed);
                await tcs.Task;
            }
            finally
            {
                movementController.DestinationReached -= Complete;
                combatController.OnDeath -= Cancel;
            }
        }
    }
}
