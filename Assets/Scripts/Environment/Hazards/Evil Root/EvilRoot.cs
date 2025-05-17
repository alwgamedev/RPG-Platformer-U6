using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class EvilRoot : PoolableObject
    {
        [SerializeField] float dormantLengthScale = 0.1f;
        [SerializeField] float emergedLengthScale = 2.5f;
        [SerializeField] float emergeMoveTime = 2.5f;
        [SerializeField] float emergeGrowTime = 1;
        [SerializeField] float retreatMoveTime = 1;
        [SerializeField] float retreatGrowTime = 1.5f;
        [SerializeField] Transform dormantHeadPosition;
        [SerializeField] Transform emergedHeadPosition;
        [SerializeField] CurveIKEffect followGuideIK;
        [SerializeField] CurveIKEffect followPlayerIK;
        [SerializeField] TriggerColliderMessenger head;
        [SerializeField] float grabRange = 3;
        [SerializeField] float grabAttemptTimeOut = 1;
        [SerializeField] float grabSpeed = 4;
        [SerializeField] float grabSnapBackTime = 0.1f;
        [SerializeField] float throwLength;
        [SerializeField] float throwForce;
        [SerializeField] float throwTime = 0.25f;
        [Range(0, 1)][SerializeField] float throwReleaseFraction = 0.6f;

        VisualCurveGuide vcg;
        bool headIsTouchingPlayer;
        float headRadius2;
        Transform playerParent;

        static EvilRoot RootHoldingPlayer;
        static event Action PlayerGrabbed;

        private void Awake()
        {
            vcg = GetComponent<VisualCurveGuide>();
            headRadius2 = head.GetComponent<CircleCollider2D>().radius;
            headRadius2 *= headRadius2;
        }

        private void OnEnable()
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

        public override void Configure(object parameters)
        {
            var erm = (IEvilRootManager)parameters;
            if (TryGetComponent(out ChildSortingLayer csl))
            {
                csl.dataSource = erm.transform;
            }
            ((ColliderBasedCurveBounds)vcg.bounds).prohibitedZone = erm.Platform;
        }

        public override void ResetPoolableObject() { }

        public void SetEmergePosition(Vector2 position)
        {
            emergedHeadPosition.position = position;
        }

        //BASIC FUNCTIONS

        public async void OnDeploy(bool throwRight, CancellationToken token)
        {
            await Emerge(token);
            if (PlayerInGrabRange())
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

                try
                {
                    PlayerGrabbed += cts.Cancel;
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
                    await ThrowPlayer(throwRight, token);
                }
            }
            await Retreat(token);
            ReturnToPool();
        }

        public async Task Emerge(CancellationToken token)
        {
            float emergeTimeMult = MiscTools.RandomFloat(0.75f, 3);
            var a = vcg.LerpLengthScale(emergedLengthScale, emergeTimeMult * emergeGrowTime, token);
            var b = followGuideIK.LerpBetweenTransforms(dormantHeadPosition, 
                emergedHeadPosition, emergeTimeMult * emergeMoveTime, token);
            await Task.WhenAll(a, b);
        }

        public async Task Retreat(CancellationToken token)
        {
            var b = vcg.LerpLengthScale(dormantLengthScale, retreatGrowTime, token);
            var a = followGuideIK.LerpTowardsTransform(dormantHeadPosition, retreatMoveTime, token);
            await Task.WhenAll(a, b);
        }

        public async Task GrabPlayer(CancellationToken token)
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

        public async Task<bool> ReachForPlayer(CancellationToken token)
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

        public async Task ThrowPlayer(bool throwRight, CancellationToken token)
        {
            float angle = MiscTools.RandomFloat(Mathf.PI / 16, 3 * Mathf.PI / 16);
            if (!throwRight)//throw opposite direction head is "facing"
            {
                angle = MathF.PI - angle;
            }
            
            Vector2 o = head.transform.position;
            var d = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var f = throwForce * d;
            d *= throwLength;

            await followGuideIK.LerpTowardsPosition(o + throwReleaseFraction * d, 
                throwReleaseFraction * throwTime, 
                token);

            ReleasePlayer(f);

            d.y *= - 1;//easy way to give the throw an arcing motion
            await followGuideIK.LerpTowardsPosition(o + d, (1 - throwReleaseFraction) * throwTime,
                token);

            RootHoldingPlayer = null;
        }

        private void AnchorPlayer()
        {
            var player = GlobalGameTools.Instance.Player;
            playerParent = GlobalGameTools.Instance.PlayerTransform.parent;
            var playerMover = ((AdvancedMover)((IMovementController)player.MovementController).Mover);
            var playerRb = playerMover.Rigidbody;
            ((IInputDependent)player).InputSource.DisableInput();
            playerRb.bodyType = RigidbodyType2D.Kinematic;
            playerRb.linearVelocity = Vector2.zero;
            playerMover.ResetJumpNum();
            playerRb.transform.SetParent(head.transform);
        }

        private void ReleasePlayer(Vector2 force)
        {
            var player = GlobalGameTools.Instance.Player;
            var playerRb = ((Mover)((IMovementController)player.MovementController).Mover).Rigidbody;
            playerRb.transform.SetParent(playerParent);
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            playerRb.AddForce(force * playerRb.mass, ForceMode2D.Impulse);
            ((IInputDependent)player).InputSource.EnableInput();
        }

        //IK SETTINGS

        private void FollowGuide()
        {
            if (!followGuideIK || !followPlayerIK)
                return;

            followPlayerIK.enabled = false;
            followGuideIK.enabled = true;
        }

        private void FollowPlayer()
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

        private void DeactivateAllIK()
        {
            followGuideIK.enabled = false;
            followPlayerIK.enabled = false;
        }


        //CAN GRAB PLAYER?

        private bool IsPlayer(Collider2D c)
        {
            return GlobalGameTools.Instance.PlayerTransform 
                && c.transform == GlobalGameTools.Instance.PlayerTransform;
        }

        private bool InHoldRange(Transform t)
        {
            return Vector2.SqrMagnitude(t.position - head.transform.position) < headRadius2;
        }

        private bool InGrabRange(Transform t)
        {
            return Vector2.SqrMagnitude(t.position - head.transform.position) < grabRange * grabRange;
        }

        private bool PlayerInGrabRange()
        {
            return InGrabRange(GlobalGameTools.Instance.PlayerTransform);
        }

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

        private void OnDisable()
        {
            head.TriggerEnter -= OnHeadTriggerEnter;
            head.TriggerStay -= OnHeadTriggerStay;
            head.TriggerExit -= OnHeadTriggerExit;

            if (RootHoldingPlayer == this)
            {
                RootHoldingPlayer = null;
            }
        }

        private void OnDestroy()
        {
            PlayerGrabbed = null;
        }
    }
}