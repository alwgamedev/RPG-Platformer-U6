﻿using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    [RequireComponent(typeof(AICombatController))]
    [RequireComponent(typeof(EarthwormMovementController))]
    public class EarthwormDriver : StateDriver, IInputDependent
    {
        [SerializeField] float emergeMoveSpeed = .5f;
        [SerializeField] float retreatMoveSpeed = 2;
        [SerializeField] float tunnelMoveSpeed = 8;
        [SerializeField] float maxTimeAboveGround = 10;
        [SerializeField] Collider2D[] contactColliders;
        [SerializeField] Transform underGroundBodyAnchor;
        [SerializeField] Transform aboveGroundBodyAnchor;
        [SerializeField] ParticleSystem slamDustParticles;
        [SerializeField] ParticleSystem slamRockParticles;
        [SerializeField] EarthwormWormhole wormhole;

        AICombatController combatController;
        EarthwormMovementController movementController;
        VisualCurveGuide curveGuide;
        bool wormholeTriggerEnabled;

        CurveIKEffect stabIKEffect => curveGuide.ikEffects[0];
        CurveIKEffect slamBodyIKEffect => curveGuide.ikEffects[1];
        CurveIKEffect slamNoseIKEffect => curveGuide.ikEffects[2];
        PolygonCollider2D GroundCollider => movementController.GroundCollider;
        float GroundLeftBound => movementController.GroundLeftBound;//giving a little padding for safety
        float GroundRightBound => movementController.GroundRightBound;
        float GroundTopBound => movementController.GroundTopBound;
        //^higher than any point on the groundCollider (so if we take ClosestPoint
        //from a point at this height, we will always get something on the top side of the ground)
        Vector3 BodyAnchorOffset => movementController.BodyAnchorOffset;
        public IInputSource InputSource { get; private set; }

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

        //private void Start()
        //{
        //    //combatController.OnDeath += OnDeath;//we just need to disable colliders which we'll do in anim event
        //    //combatController.OnRevive += OnRevive; //it won't be revived 
        //}

        public void InitializeState()
        {
            CurrentTarget = GlobalGameTools.Instance.Player.Combatant.Health;
            GlobalGameTools.PlayerDeath += TriggerDormant;

            stabIKEffect.SetTarget(CurrentTarget.transform);
            //curveGuide.ReconfigureIKEffects();
            DisableAllIK();

            SetBodyAnchor(false);
            ChooseRandomWormholePosition();
            GoToWormhole();
            SetAutoRetaliate(false);
            SetInvincible(true);
            Trigger(typeof(EarthwormDormant).Name);
        }

        public void InitializeInputSource()
        {
            InputSource = GetComponent<IInputSource>();
        }

        public void OnInputEnabled() { }

        public void OnInputDisabled()
        {
            DisableAllIK();
        }


        //SETTINGS

        public void EnableContactColliders()
        {
            foreach (var c in contactColliders)
            {
                if (c)
                {
                    c.enabled = true;
                }
            }
        }

        public void DisableContactColliders()
        {
            foreach (var c in contactColliders)
            {
                if (c)
                {
                    c.enabled = false;
                }
            }
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
        //wormhole being triggered will automatically disable the trigger,
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

        public void SetHitEffectsActive(bool val)
        {
            var t = combatController.Combatant.Health.HitEffectTransform;
            if (t != transform && val != t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(val);
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

        private void TriggerDormant()
        {
            Trigger(typeof(EarthwormDormant).Name);
        }


        //EFFECTS

        public void DisableAllIK()
        {
            curveGuide.DisableAllIK();
        }

        //call from anim event
        public void PrepareSlamEffects()
        {
            var right = ((int)movementController.CurrentOrientation) * Vector3.right;
            var bodyTarget = transform.position + 0.75f * right - 0.5f * Vector3.up;
            //(even though this y-value will be changed, we should have a reliable one in there in case raycast fails)

            var noseTarget = transform.position + 1.75f * right - 0.5f * Vector3.up;

            var oB = new Vector2(bodyTarget.x, GroundTopBound);
            var oN = new Vector2(noseTarget.x, GroundTopBound);
            var oP = 0.75f * oB + 0.25f * oN;

            var hitB = Physics2D.Raycast(oB, -Vector2.up, Mathf.Infinity, movementController.GroundLayer);
            var hitN = Physics2D.Raycast(oN, -Vector2.up, Mathf.Infinity, movementController.GroundLayer);
            var hitP = Physics2D.Raycast(oP, -Vector2.up, Mathf.Infinity, movementController.GroundLayer);

            slamBodyIKEffect.SetTarget(hitB.point + 0.25f * Vector2.up);
            slamNoseIKEffect.SetTarget(hitN.point + 0.25f * Vector2.up);
            //Debug.DrawLine(hitB.point, hitN.point, Color.red, 5);

            slamDustParticles.transform.position = hitP.point;
            slamRockParticles.transform.position = hitP.point;
        }

        //call from anim event
        public void PlaySlamParticles()
        {
            slamDustParticles.Play();
            slamRockParticles.Play();
        }

        public void PlayDeathParticles()
        {
            slamDustParticles.Stop();
            slamDustParticles.transform.position = aboveGroundBodyAnchor.position + 0.1f * Vector3.up;
            slamDustParticles.Play();
        }

        public async void BeginDeathRotation()
        {
            await movementController
                .RotateContinuouslyTowardsGroundDirection(GlobalGameTools.Instance.TokenSource.Token);
        }


        //STATE BEHAVIORS

        public void AboveGroundBehavior()
        {
            if (CurrentTarget != null && !combatController.Combatant.CanAttack(CurrentTarget)
                && !combatController.ChannelingAbility)
            {
                if (CanPursue())
                {
                    Trigger(typeof(EarthwormPursuit).Name);
                }
                else
                {
                    Trigger(typeof(EarthwormDormant).Name);
                }
            }
            else if (CurrentTarget != null && !CurrentTarget.IsDead && !combatController.Attacking)
            {
                StartAttacking();
            }
        }

        public async Task AboveGroundTimer(CancellationToken token)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            try
            {
                combatController.OnDeath += cts.Cancel;
                await MiscTools.DelayGameTime(maxTimeAboveGround, cts.Token);
            }
            finally
            {
                combatController.OnDeath -= cts.Cancel;
            }
        }

        public void AboveGroundTimerComplete()
        {
            Trigger(typeof(EarthwormRetreat).Name);
        }

        public bool CanPursue()
        {
            return CurrentTarget != null && !CurrentTarget.IsDead
                && CurrentTarget.transform.position.x > GroundLeftBound
                && CurrentTarget.transform.position.x < GroundRightBound;
        }

        public async Task PursueTarget(CancellationToken token)
        {
            var x = Mathf.Clamp(CurrentTarget.transform.position.x, movementController.GroundLeftBound,
                movementController.GroundRightBound);
            var p = new Vector2(x, GroundTopBound);
            var q = GroundCollider.ClosestPoint(p);
            SetWormholePosition(q);
            await TunnelTowardsWormhole(token);
        }

        //making this separate so that handler of the pursuit -> aboveground transition 
        //doesn't get wrapped up in the previous x -> pursuit transition
        public void PursuitComplete()
        {
            Trigger(typeof(EarthwormAboveGround).Name);
        }


        //COMBAT

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
            ChooseRandomWormholePosition(GroundLeftBound, GroundRightBound);
        }

        public void ChooseRandomWormholePosition(float leftXBd, float rightXBd)
        {
            var x = MiscTools.RandomFloat(leftXBd, rightXBd);
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

        //cancellation needs to be handled higher up
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

        public async Task TunnelTowardsWormhole(CancellationToken token)
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

        //private void OnDeath()
        //{
        //    DisableContactColliders();
        //}

        //private void OnRevive()
        //{
        //    EnableContactColliders();
        //}

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (wormhole && wormhole.gameObject)
            {
                Destroy(wormhole.gameObject);
            }
            if (slamDustParticles && slamDustParticles.gameObject)
            {
                Destroy(slamDustParticles.gameObject);
            }
            if (slamRockParticles && slamRockParticles.gameObject)
            {
                Destroy(slamRockParticles.gameObject);
            }

            GlobalGameTools.PlayerDeath -= TriggerDormant;
        }
    }
}
