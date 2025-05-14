using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class EvilRoot : MonoBehaviour
    {
        [SerializeField] float dormantLengthScale = 0.1f;
        [SerializeField] float emergedLengthScale = 2.5f;
        [SerializeField] float emergeTime = 2.5f;
        [SerializeField] float retreatTime = 1;
        [SerializeField] Transform dormantHeadPosition;
        [SerializeField] Transform emergedHeadPosition;
        [SerializeField] CurveIKEffect followGuideIK;
        [SerializeField] CurveIKEffect followPlayerIK;
        [SerializeField] TriggerColliderMessenger head;
        [SerializeField] float grabTimeOut = 1;
        [SerializeField] float grabSpeed = 4;
        [SerializeField] float throwLength;
        [SerializeField] float throwForce;
        [SerializeField] float throwTime = 0.25f;
        [Range(0, 1)][SerializeField] float throwReleaseFraction = 0.6f;

        VisualCurveGuide vcg;
        bool headIsTouchingPlayer;
        float headRadius2;
        Transform playerParent;

        System.Random rng = new();

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

            vcg.lengthScale = dormantLengthScale;
            FollowGuide();
        }


        //BASIC FUNCTIONS

        public async Task Emerge()
        {
            var a = followGuideIK.LerpBetweenTransforms(dormantHeadPosition, emergedHeadPosition, emergeTime);
            var b = vcg.LerpLengthScale(emergedLengthScale, emergeTime);
            await Task.WhenAll(a, b);
            //hopefully task.whenall will do what i hope it does and run these "in parallel"; we'll see
        }

        public async Task Retreat()
        {
            var a = followGuideIK.LerpTowardsTransform(dormantHeadPosition, retreatTime);
            var b = vcg.LerpLengthScale(dormantLengthScale, retreatTime);
            await Task.WhenAll(a, b);
        }

        public async Task GrabPlayer()
        {
            Task<bool> reach = ReachForPlayer();
            await reach;

            if (reach.Result)
            {
                DeactivateAllIK();
                AnchorPlayer();
            }

            //lerp back to position of ik guide (which will be wherever it was when we started the grab)
            FollowGuide();
            await followGuideIK.LerpBetweenPositions(head.transform.position, followGuideIK.TargetPosition(),
                1 / grabSpeed);
        }

        public async Task<bool> ReachForPlayer()
        {
            float timer = 0;
            followPlayerIK.ikStrength = 0;
            FollowPlayer();

            while (timer < grabTimeOut)
            {
                if (headIsTouchingPlayer)
                    return true;

                await Task.Yield(); 
                if (GlobalGameTools.Instance.TokenSource.IsCancellationRequested)
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

        public async Task ThrowPlayer()
        {
            float angle = (float)rng.NextDouble() * Mathf.PI / 8 + Mathf.PI / 16;
            if (rng.Next(0, 2) == 1)
            {
                angle = Mathf.PI - angle;
            }
            var d = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var f = throwForce * d;
            d *= throwLength;

            await followGuideIK.LerpTowardsPosition((Vector2)head.transform.position 
                + throwReleaseFraction * d, throwTime);

            ReleasePlayer(f);

            d.y *= -1;//we'll see how this looks; trying to give the throw an arcing motion
            await followGuideIK.LerpTowardsPosition((Vector2)head.transform.position 
                + (1 - throwReleaseFraction) * d, throwTime);
        }

        private void AnchorPlayer()
        {
            Debug.Log("anchoring");
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
            Debug.Log("releasing player");
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
            return Vector3.SqrMagnitude(t.position - head.transform.position) < headRadius2;
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
        }
    }
}