using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class EvilRoot : PoolableObject
    {
        [SerializeField] protected float dormantLengthScale = 0.1f;
        [SerializeField] protected float emergedLengthScale = 2.5f;
        [SerializeField] protected float emergeGrowTime = 1;
        [SerializeField] protected float retreatMoveTime = 1;
        [SerializeField] protected float retreatGrowTime = 1.5f;
        [SerializeField] protected Transform dormantHeadPosition;
        [SerializeField] protected Transform emergedHeadPosition;
        [SerializeField] protected CurveIKEffect followGuideIK;
        [SerializeField] protected CurveIKEffect followPlayerIK;
        [SerializeField] protected TriggerColliderMessenger head;
        [SerializeField] protected float grabRange = 3;
        [SerializeField] protected float grabAttemptTimeOut = 1;
        [SerializeField] protected float grabSpeed = 4;
        [SerializeField] protected float grabSnapBackTime = 0.1f;
        [SerializeField] protected float throwLength;
        [SerializeField] protected float throwForce;
        [SerializeField] protected float throwTime = 0.25f;
        [Range(0, 1)][SerializeField] protected float throwReleaseFraction = 0.6f;

        protected VisualCurveGuide vcg;
        protected bool headIsTouchingPlayer;
        protected float headRadius2;
        protected bool hasThrown;
        protected Transform playerParent;

        protected event Action Destroyed;

        public static EvilRoot RootHoldingPlayer { get; protected set; }
        public static event Action PlayerGrabbed;

        protected virtual void Awake()
        {
            vcg = GetComponent<VisualCurveGuide>();
            headRadius2 = head.GetComponent<CircleCollider2D>().radius;
            headRadius2 *= headRadius2;
        }

        protected virtual void OnEnable()
        {
            head.TriggerEnter += OnHeadTriggerEnter;
            head.TriggerStay += OnHeadTriggerStay;
            head.TriggerExit += OnHeadTriggerExit;

            FollowGuide();
        }

        private void Start()
        {
            vcg.lengthScale = dormantLengthScale;
            vcg.CallUpdate();
        }

        public override void BeforeSetActive() { }

        public override void AfterSetActive() { }

        public override void Configure(object parameters)
        {
            var erm = (IEvilRootManager)parameters;
            if (TryGetComponent(out ChildSortingLayer csl))
            {
                csl.dataSource = erm.RootSortingLayerDataSource;
                csl.Validate();
            }
            var b = (ColliderBasedCurveBounds)vcg.bounds;
            if (b)
            {
                b.prohibitedZone = erm.Platform;
            }
        }

        public override void ResetPoolableObject()
        {
            hasThrown = false;
        }

        public void SetEmergePosition(Vector2 position)
        {
            emergedHeadPosition.position = position;
        }

        //BASIC FUNCTIONS

        public virtual async void OnDeploy(CancellationToken token)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            try
            {
                Destroyed += cts.Cancel;
                var a = vcg.LerpLengthScale(emergedLengthScale, emergeGrowTime,
                cts.Token, HasNotThrownYet);
                var b = Emerge(cts.Token);
                await Task.WhenAll(a, b);
                await Retreat(cts.Token);
                ReturnToPool();
            }
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                Destroyed -= cts.Cancel;
            }

        }

        public virtual async Task Emerge(CancellationToken token)
        {
            await Emerge(false, true, true, token);
        }

        public virtual async Task Emerge(bool useEmergePosition, bool cancelIfAnotherRootGrabs, bool throwIfGrabSucceeds,
            CancellationToken token)
        {
            if (useEmergePosition)
            {
                await followGuideIK.LerpTowardsTransform(emergedHeadPosition, emergeGrowTime, token);
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            try
            {
                if (cancelIfAnotherRootGrabs)
                {
                    PlayerGrabbed += cts.Cancel;
                }
                await GrabPlayer(cts.Token);
            }
            catch (TaskCanceledException) { }
            finally
            {
                PlayerGrabbed -= cts.Cancel;
            }

            if (RootHoldingPlayer == this)
            {
                PlayerGrabbed?.Invoke();
                if (throwIfGrabSucceeds)
                {
                    await ThrowPlayer(ThrowRight(), token);
                }
            }
        }

        public virtual async Task Retreat(CancellationToken token)
        {
            var b = vcg.LerpLengthScale(dormantLengthScale, retreatGrowTime, token);
            var a = followGuideIK.LerpTowardsTransform(dormantHeadPosition, retreatMoveTime, token);
            await Task.WhenAll(a, b);
        }

        public virtual async Task GrabPlayer(CancellationToken token)
        {
            Vector2 p = followGuideIK.TargetPosition();
            Task<bool> reach = ReachForPlayer(token);
            await reach;

            if (reach.Result && !RootHoldingPlayer)
            {
                RootHoldingPlayer = this;
                DeactivateAllIK();
                AnchorPlayer();
                FollowGuide();
                await followGuideIK.LerpBetweenPositions(head.transform.position, p,
                    grabSnapBackTime, token);
            }
            else
            {
                FollowGuide();
            }
        }

        public virtual async Task<bool> ReachForPlayer(CancellationToken token)
        {
            float timer = 0;
            followPlayerIK.ikStrength = 0;
            FollowPlayer();

            while (timer < grabAttemptTimeOut)
            {
                if (headIsTouchingPlayer)
                    return true;

                await Task.Yield(); 
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                timer += Time.deltaTime;
                if (followPlayerIK.ikStrength < 1)
                {
                    followPlayerIK.ikStrength += grabSpeed * Time.deltaTime;
                }
            }

            return headIsTouchingPlayer;
        }

        public virtual async Task ThrowPlayer(bool throwRight, CancellationToken token)
        {
            hasThrown = true;//so that we stop growing lengthScale at this point
            float angle = MiscTools.RandomFloat(Mathf.PI / 16, 3 * Mathf.PI / 16);
            if (!throwRight)
            {
                angle = MathF.PI - angle;
            }
            
            Vector2 o = head.transform.position;
            var d = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var f = throwForce * d;
            var t = throwReleaseFraction * throwTime;
            d *= throwLength;

            await followGuideIK.LerpTowardsPosition(o + throwReleaseFraction * d, t, token);
            ReleasePlayer(f);

            d.y *= - 1;//to give the throw an arcing motion
            await followGuideIK.LerpTowardsPosition(o + d, throwTime - t,
                token);

            RootHoldingPlayer = null;
        }

        protected void AnchorPlayer()
        {
            var player = GlobalGameTools.Instance.Player;
            playerParent = GlobalGameTools.Instance.PlayerTransform.parent;
            var playerMover = (AdvancedMover)GlobalGameTools.Instance.PlayerMover.Mover;
            var playerRb = playerMover.Rigidbody;
            ((IInputDependent)player).InputSource.DisableInput();
            playerRb.SetKinematic();
            playerMover.ResetJumpNum();
            playerRb.transform.SetParent(head.transform);
        }

        protected void ReleasePlayer(Vector2 force)
        {
            var player = GlobalGameTools.Instance.Player;
            var playerRb = ((Mover)GlobalGameTools.Instance.PlayerMover.Mover).Rigidbody;
            playerRb.transform.SetParent(playerParent);
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            playerRb.AddForce(force * playerRb.mass, ForceMode2D.Impulse);
            ((IInputDependent)player).InputSource.EnableInput();
        }

        protected bool ThrowRight()
        {
            bool throwRight = GlobalGameTools.Instance.Player.MovementController.CurrentOrientation 
                == HorizontalOrientation.left;
            var playerPosition = GlobalGameTools.Instance.PlayerTransform.position;
            var boundsCenter = ((ColliderBasedCurveBounds)vcg.bounds).prohibitedZone.bounds.center;
            if (throwRight && playerPosition.x < boundsCenter.x && playerPosition.y < boundsCenter.y)
            {
                throwRight = false;
                //prevents you from repeatedly getting thrown back into the platform when you are
                //in the "third quadrant" (relative to direction player is facing)
            }
            else if (!throwRight && playerPosition.x > boundsCenter.x && playerPosition.y < boundsCenter.y)
            {
                throwRight = true;
            }
            if (MiscTools.rng.Next() < .05)//5% chance of throwing in the wrong direction
            {
                throwRight = !throwRight;
            }

            return throwRight;
        }

        //IK SETTINGS

        protected void FollowGuide()
        {
            if (!followGuideIK || !followPlayerIK)
                return;

            followPlayerIK.enabled = false;
            followGuideIK.enabled = true;
        }

        protected void FollowPlayer()
        {
            if (!followGuideIK || !followPlayerIK)
                return;

            followGuideIK.enabled = false;

            if (GlobalGameTools.Instance.PlayerTransform)
            {
                followPlayerIK.SetTarget(GlobalGameTools.Instance.PlayerTransform);
                followPlayerIK.enabled = true;
            }
        }

        protected void DeactivateAllIK()
        {
            followGuideIK.enabled = false;
            followPlayerIK.enabled = false;
        }


        //CAN GRAB PLAYER?

        protected bool IsPlayer(Collider2D c)
        {
            return GlobalGameTools.Instance.PlayerTransform 
                && c.transform == GlobalGameTools.Instance.PlayerTransform;
        }

        protected bool HasNotThrownYet()
        {
            return !hasThrown;
        }

        private bool InHoldRange(Transform t)
        {
            return Vector2.SqrMagnitude(t.position - head.transform.position) < headRadius2;
        }

        //private bool InGrabRange(Transform t)
        //{
        //    return Vector2.SqrMagnitude(t.position - head.transform.position) < grabRange * grabRange;
        //}

        //private bool PlayerNotInGrabRange()
        //{
        //    return !PlayerInGrabRange();
        //}

        //private bool PlayerInGrabRange()
        //{
        //    return InGrabRange(GlobalGameTools.Instance.PlayerTransform);
        //}

        private void OnHeadTriggerEnter(Collider2D collider)
        {
            if (IsPlayer(collider))
            {
                headIsTouchingPlayer = InHoldRange(collider.transform);
            }
        }

        private void OnHeadTriggerStay(Collider2D collider)
        {
            if (IsPlayer(collider))
            {
                headIsTouchingPlayer = InHoldRange(collider.transform);
            }
        }

        private void OnHeadTriggerExit(Collider2D collider)
        {
            if (IsPlayer(collider))
            {
                headIsTouchingPlayer = false;
            }
        }

        protected virtual void OnDisable()
        {
            head.TriggerEnter -= OnHeadTriggerEnter;
            head.TriggerStay -= OnHeadTriggerStay;
            head.TriggerExit -= OnHeadTriggerExit;

            if (RootHoldingPlayer == this)
            {
                RootHoldingPlayer = null;
            }
        }

        protected virtual void OnDestroy()
        {
            Destroyed?.Invoke();
            Destroyed = null;
            PlayerGrabbed = null;
        }
    }
}