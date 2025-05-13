using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class EvilRoot : MonoBehaviour
    {
        [SerializeField] CurveIKEffect followGuideIK;
        [SerializeField] CurveIKEffect followPlayerIK;
        [SerializeField] TriggerColliderMessenger head;
        [SerializeField] float grabTimeOut = 1;
        [SerializeField] float grabSpeed = 4;

        bool headIsTouchingPlayer;
        float headRadius2;
        AnimationControl animator;
        Task grabTask;//don't think we need to or should dispose of it OnDestroy
        Transform playerParent;

        event Action Emerged;

        private void Awake()
        {
            animator = GetComponent<AnimationControl>();
            headRadius2 = head.GetComponent<CircleCollider2D>().radius;
            headRadius2 *= headRadius2;
        }

        private void Start()
        {
            head.TriggerEnter += OnHeadTriggerEnter;
            head.TriggerStay += OnHeadTriggerStay;
            head.TriggerExit += OnHeadTriggerExit;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                BeginNewGrabTaskIfNoneInProgress();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                ReleasePlayer();
            }
        }

        public void Emerge()
        {
            animator.SetTrigger("emerge");
        }

        //call in animation event, so we know when emerge is done
        public void CompleteEmerge()
        {
            Emerged?.Invoke();
        }

        //make sure all grab/throw tasks are completed before you call this
        //I don't want to make this async (i.e. waiting for grabTask before we start)
        //because then we may have to worry about retreat getting called when retreat is already in progress
        public void Retreat()
        {
            animator.SetTrigger("retreat");

        }

        public async void BeginNewGrabTaskIfNoneInProgress()
        {
            if (grabTask == null || grabTask.IsCompleted)
            {
                grabTask = GrabPlayer();
                await grabTask;
            }
        }

        public async Task GrabPlayer()
        {
           Task<bool> reach = ReachForPlayer();
            await reach;

            Debug.Log($"reach successful? {reach.Result}");

            if (reach.Result)
            {
                DeactivateAllIK();
                AnchorPlayer();
            }

            await LerpBackToGuidePosition(grabSpeed);
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

        private void ReleasePlayer()
        {
            Debug.Log("releasing player");
            var player = GlobalGameTools.Instance.Player;
            var playerRb = ((Mover)((IMovementController)player.MovementController).Mover).Rigidbody;
            playerRb.transform.SetParent(playerParent);
            playerRb.bodyType = RigidbodyType2D.Dynamic;
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

        private async Task LerpBackToGuidePosition(float lerpRate)
        {
            Vector2 guidePosition = followGuideIK.IKTargetTransform.position;
            Vector2 headPosition = head.transform.position;
            followGuideIK.IKTargetTransform.position = headPosition;
            FollowGuide();

            float t = 0;

            while (t < 1)
            {
                await Task.Yield();

                if (GlobalGameTools.Instance.TokenSource.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                t += Time.deltaTime * lerpRate;
                followGuideIK.IKTargetTransform.position = Vector2.Lerp(headPosition, guidePosition, t);
            }
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

        public void DeactivateAllIK()
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

        private void OnDestroy()
        {
            Emerged = null;
        }
    }
}