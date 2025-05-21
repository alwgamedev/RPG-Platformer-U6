using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class ChainedMover : StateDriver, IMover
    {
        [SerializeField] protected Rigidbody2D[] bodyPieces;
        [SerializeField] protected MovementOptions movementOptions;
        [SerializeField] protected float walkSpeed;
        [SerializeField] protected float runSpeed;
        [SerializeField] protected float maxDistanceCorrectionSpeed;
        [SerializeField] protected float distanceCorrectionScale;

        //protected Vector3 centerOffset;//offset of "ColliderCenter" from transform.position
        protected int groundLayer;
        protected float[] goalDistances;
        protected Vector2[] positions;
        protected Vector2[] groundDirections;//(always pointing left to right, normalized)

        protected float moveInput;

        public int NumBodyPieces => bodyPieces.Length;
        public Rigidbody2D[] BodyPieces => bodyPieces;
        public HorizontalOrientation CurrentOrientation { get; protected set; }
        public bool FacingRight => CurrentOrientation == HorizontalOrientation.right;
        public float MaxSpeed => Running ? runSpeed : walkSpeed;
        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public bool Running { get; set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public Vector3 CenterPosition => transform.position;

        public event Action<HorizontalOrientation> DirectionChanged;

        protected virtual void Awake()
        {
            goalDistances = new float[NumBodyPieces - 1];
            positions = new Vector2[NumBodyPieces];
            groundDirections = new Vector2[NumBodyPieces];

            //Width = 0;
            for (int i = 0; i < NumBodyPieces - 1; i++)
            {
                goalDistances[i] = Vector2.Distance(bodyPieces[i].position, bodyPieces[i + 1].position);
                Width += goalDistances[i];
            }

            var c = bodyPieces[0].GetComponent<Collider2D>();
            Height = c.bounds.max.y - c.bounds.min.y;

            //centerOffset = 0.5f * (c.bounds.center
            //    + bodyPieces[NumBodyPieces - 1].GetComponent<Collider2D>().bounds.center)
            //    - transform.position;

            groundLayer = LayerMask.GetMask("Ground");
        }

        protected virtual void FixedUpdate()
        {
            EnforceDistances();
        }

        public virtual void HandleMoveInput(Vector3 moveInput)
        {
            SetOrientation(moveInput);
            UpdatePhysicsData();
            Move(moveInput);
        }

        public virtual void Stop()
        {
            foreach (var b in bodyPieces)
            {
                b.linearVelocity = Vector2.zero;
            }
        }

        public void SetPosition(Vector3 position)
        {
            foreach (var b in bodyPieces)
            {
                b.SetKinematic();
            }

            for (int i = 0; i < NumBodyPieces; i++)
            {
                if (bodyPieces[i].transform == transform) continue;
                bodyPieces[i].transform.position = position + (bodyPieces[i].transform.position - transform.position);
            }

            transform.position = position;

            foreach (var b in bodyPieces)
            {
                b.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        protected virtual void Move(Vector3 moveInput)
        {
            if (moveInput.x != 0)
            {
                moveInput.x = Mathf.Sign(moveInput.x);

                for (int i = 0; i < NumBodyPieces; i++)
                {
                    bodyPieces[i].Move(FacingRight, moveInput.z < 0, bodyPieces[i].linearVelocity, 
                        moveInput.x * groundDirections[i], MaxSpeed, movementOptions);
                }
            }
        }

        public virtual void SetOrientation(Vector3 moveInput)
        {
            if (moveInput.x != 0)
            {
                SetOrientation((HorizontalOrientation)(Math.Sign(moveInput.x) * Mathf.Sign(moveInput.z)));
            }
        }

        public virtual void SetOrientation(HorizontalOrientation o)
        {
            if (o != CurrentOrientation)
            {
                CurrentOrientation = o;
                ChangeDirection();
                DirectionChanged?.Invoke(CurrentOrientation);
            }
        }

        protected virtual void ChangeDirection()
        {
            Vector3 s;

            for (int i = 0; i < NumBodyPieces; i++)
            {
                positions[i] = bodyPieces[i].position;
            }
            for (int i = 0; i < NumBodyPieces; i++)
            {
                bodyPieces[i].position = positions[NumBodyPieces - i - 1];
                s = bodyPieces[i].transform.localScale;
                s.x *= -1;
                bodyPieces[i].transform.localScale = s;
            }
        }

        protected virtual void UpdatePhysicsData()
        {
            UpdateGroundDirections();
            UpdateRotation();
            //EnforceDistances();
        }

        protected void EnforceDistances()
        {
            Vector2 d;
            Vector2 m;
            float r;

            for (int i = 0; i < NumBodyPieces - 1; i++)
            {
                d = bodyPieces[i].position - (int)CurrentOrientation * groundDirections[i] * goalDistances[i];
                //^goal position for bodyPieces[i + 1]
                d = d - bodyPieces[i + 1].position;
                m = ((int)CurrentOrientation) * groundDirections[i + 1];
                r = Mathf.Clamp(Vector2.Dot(bodyPieces[i + 1].linearVelocity, m), 0, MaxSpeed);
                m = bodyPieces[i + 1].linearVelocity - r * m;
                r = Vector2.Dot(m, d.normalized);

                if (r < maxDistanceCorrectionSpeed)
                {
                    //this made it a lot smoother (worth performance cost)
                    for (int j = i + 1; j < NumBodyPieces; j++)
                    {
                        bodyPieces[j].AddForce(bodyPieces[j].mass * distanceCorrectionScale * d);
                    }
                }
            }
        }

        protected void UpdateRotation()
        {
            for (int i = 0; i < NumBodyPieces; i++)
            {
                bodyPieces[i].transform
                    .RotateTowardsMovementDirection(FacingRight, (int)CurrentOrientation * groundDirections[i], 
                    movementOptions);
            }
        }

        protected void UpdateGroundDirections()
        {
            for (int i = 0; i < NumBodyPieces; i++)
            {
                groundDirections[i] = GroundDirectionVector(bodyPieces[i]);
            }
        }

        //this (and analogous methods in Mover) could be moved to static Physics tools
        //(also you should correct how it compute right and left groundcast, i.e. collider bounds or someth
        //-- not super important)
        protected Vector2 GroundDirectionVector(Rigidbody2D body)
        {

            return (RightGroundcast(body).point - LeftGroundcast(body).point).normalized;
        }

        protected RaycastHit2D RightGroundcast(Rigidbody2D body)
        {
            var o = new Vector2(body.position.x + 0.1f, body.position.y);
            return Physics2D.Raycast(o, -Vector2.up, Mathf.Infinity, groundLayer);
        }

        protected RaycastHit2D LeftGroundcast(Rigidbody2D body)
        {
            var o = new Vector2(body.position.x - 0.1f, body.position.y);
            return Physics2D.Raycast(o, -Vector2.up, Mathf.Infinity, groundLayer);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DirectionChanged = null;
        }
    }

}