﻿using System;
using System.Threading;
using System.Threading.Tasks;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using UnityEngine;

namespace RPGPlatformer.AIControl
{
    public class EarthwormMovementController : MonoBehaviour, ICombatantMovementController, IEntityOrienter, 
        IInputDependent
    {
        //[SerializeField] float moveSpeed = 0.5f;
        [SerializeField] float destinationTolerance = 0.1f;
        [SerializeField] ParticleSystem tunnelingParticles;
        [SerializeField] PolygonCollider2D groundCollider;
        [SerializeField] RandomizableVector2 groundBounds;
        [SerializeField] float rotationalSpeed = 3;

        Vector3 destination;
        float moveSpeed;
        int groundLayer;
        Action MoveAction;

        public Transform bodyAnchor;

        public IInputSource InputSource { get; private set; }
        public bool Moving => false;
        public HorizontalOrientation CurrentOrientation => (HorizontalOrientation)Mathf.Sign(transform.localScale.x);
        public Vector3 BodyAnchorOffset => bodyAnchor.position - transform.position;
        public PolygonCollider2D GroundCollider => groundCollider;
        public int GroundLayer => groundLayer;
        public float GroundLeftBound => groundBounds.Min.x;//groundCollider.bounds.min.x + 0.5f;//giving a little padding for safety
        public float GroundRightBound => groundBounds.Max.x;//groundCollider.bounds.max.x - 0.5f;
        public float GroundTopBound => groundBounds.Max.y;//groundCollider.bounds.center.y + groundCollider.bounds.size.y;
        //higher than any point on the ground collider

        event Action Destroyed;
        public event Action DestinationReached;
        public event Action<HorizontalOrientation> DirectionChanged;

        private void Awake()
        {
            groundLayer = LayerMask.GetMask("Ground");
        }

        private void FixedUpdate()
        {
            MoveAction?.Invoke();
        }

        public void InitializeInputSource()
        {
            InputSource = GetComponent<IInputSource>();
        }

        public void GoTo(Vector3 point)
        {
            transform.position = point;
        }

        public void BeginMoveTowards(Vector3 destination, float moveSpeed)
        {
            this.destination = destination;
            this.moveSpeed = moveSpeed;
            MoveAction = MoveTowardsDestination;
        }

        public void BeginMoveAnchored(Vector3 destination, float moveSpeed)
        {
            this.destination = destination;
            this.moveSpeed = moveSpeed;
            MoveAction = MoveAnchored;
        }

        public void PlayTunnelingParticles()
        {
            tunnelingParticles.Play();
        }

        public void MoveTowardsDestination()
        {
            var d = destination - transform.position;
            var l = d.magnitude;

            if (l < destinationTolerance)
            {
                Stop();
                DestinationReached?.Invoke();
            }
            else
            {
                transform.position += moveSpeed * Time.deltaTime * (d / l);
            }
        }

        public void MoveAnchored()
        {
            var d = destination.x - bodyAnchor.position.x;

            if (Mathf.Abs(d) < destinationTolerance)
            {
                Stop();
                DestinationReached?.Invoke();
            }
            else
            {
                var dir = Mathf.Sign(d);
                var p = new Vector2(transform.position.x + dir * moveSpeed * Time.deltaTime,
                    GroundTopBound);
                var r = Physics2D.Raycast(p, -Vector2.up, Mathf.Infinity, groundLayer);
                //polygonCollider.ClosestPoint doesn't work, because it only gets vertices of the polygon,
                //and move increment might not be big enough to move to a different point
                if (r)
                {
                    transform.position = (Vector3)r.point - BodyAnchorOffset;
                }
            }
        }

        public void Stop()
        {
            tunnelingParticles.Stop();
            MoveAction = null;
        }

        public void FaceTarget(Transform target)
        {
            if (target)
            {
                FaceTarget(target.position);
            }
        }

        public void FaceTarget(Vector3 target)
        {
            var d = target.x - transform.position.x;

            if (d != 0)
            {
                var s = transform.localScale;
                s.x = Mathf.Sign(d) * Mathf.Abs(s.x);
                transform.localScale = s;
                DirectionChanged?.Invoke(CurrentOrientation);
            }
        }

        public async Task RotateContinuouslyTowardsGroundDirection(CancellationToken token)
        {
            //all looks pretty silly, but I just want to be safe
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            try
            {
                Destroyed += cts.Cancel;
                while (transform)
                { 
                    RotateTowardsGroundDirection();
                    await Task.Yield();
                    if (cts.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }
                }
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

        public void RotateTowardsGroundDirection()
        {
            var oR = new Vector2(transform.position.x + 0.2f, GroundTopBound);
            var oL = new Vector2(transform.position.x - 0.2f, GroundTopBound);
            var hitR = Physics2D.Raycast(oR, -Vector2.up, Mathf.Infinity, groundLayer);
            var hitL = Physics2D.Raycast(oL, -Vector2.up, Mathf.Infinity, groundLayer);
            if (hitR && hitL)
            {
                var d = (hitR.point - hitL.point).normalized;
                transform.TweenTransformUpTowards(d.CCWPerp(), rotationalSpeed * Time.deltaTime);
            }
        }

        public void OnInputEnabled() { }

        public void OnInputDisabled() { }

        public void OnDeath()
        {
            Stop();   
        }

        public void OnRevival() { }

        private void OnDestroy()
        {
            Destroyed?.Invoke();

            Destroyed = null;
            MoveAction = null;
            DestinationReached = null;
            DirectionChanged = null;
        }

        
        //UNUSED INTERFACE METHODS

        public void SetRunning(bool val)
        {
            return;
        }
    }
}