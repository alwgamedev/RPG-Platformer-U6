using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class ChainedMover : MonoBehaviour
    {
        [SerializeField] protected Rigidbody2D[] bodyPieces;
        [SerializeField] protected MovementOptions movementOptions;
        [SerializeField] protected float maxSpeed;
        [SerializeField] protected float maxDistanceCorrectionSpeed;
        [SerializeField] protected float distanceCorrectionScale;

        protected int groundLayer;
        protected float[] goalDistances;
        protected Vector2[] positions;
        protected Vector2[] groundDirections;//(always pointing left to right, normalized)

        protected float moveInput;

        public int numBodyPieces => bodyPieces.Length;
        public HorizontalOrientation CurrentOrientation { get; protected set; } = HorizontalOrientation.right;
        public bool FacingRight => CurrentOrientation == HorizontalOrientation.right;
        public float Length { get; private set; }

        protected virtual void Awake()
        {
            goalDistances = new float[numBodyPieces - 1];
            positions = new Vector2[numBodyPieces];
            groundDirections = new Vector2[numBodyPieces];
            Length = 0;

            for (int i = 0; i < numBodyPieces - 1; i++)
            {
                goalDistances[i] = Vector2.Distance(bodyPieces[i].position, bodyPieces[i + 1].position);
                Length += goalDistances[i];
            }
            groundLayer = LayerMask.GetMask("Ground");
        }

        protected virtual void FixedUpdate()
        {
            UpdatePhysicsData();

            //(just for testing for now)
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                moveInput = -1;
                if (FacingRight)
                {
                    ChangeDirection();
                }
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                moveInput = 1;
                if (!FacingRight)
                {
                    ChangeDirection();
                }
            }
            else
            {
                moveInput = 0;
            }

            HandleMoveInput(moveInput);
        }

        protected virtual void UpdatePhysicsData()
        {
            UpdateGroundDirections();
            UpdateRotation();
            EnforceDistances();
        }



        protected virtual void HandleMoveInput(float moveInput)
        {
            if (moveInput != 0)
            {
                Rigidbody2D rb;

                for (int i = 0; i < numBodyPieces; i++)
                {
                    rb = bodyPieces[i];
                    rb.Move(FacingRight, rb.linearVelocity, moveInput * groundDirections[i],
                        maxSpeed, movementOptions);
                }
            }
        }

        protected void EnforceDistances()
        {
            Vector2 d;
            Vector2 m;
            float r;

            for (int i = 0; i < numBodyPieces - 1; i++)
            {
                d = bodyPieces[i].position - (int)CurrentOrientation * groundDirections[i] * goalDistances[i];
                //^goal position for bodyPieces[i + 1]
                d = d - bodyPieces[i + 1].position;
                m = ((int)CurrentOrientation) * groundDirections[i + 1];
                r = Mathf.Clamp(Vector2.Dot(bodyPieces[i + 1].linearVelocity, m), 0, maxSpeed);
                m = bodyPieces[i + 1].linearVelocity - r * m;
                r = Vector2.Dot(m, d.normalized);

                if (r < maxDistanceCorrectionSpeed)
                {
                    //this made it a lot smoother (worth performance cost)
                    for (int j = i + 1; j < numBodyPieces; j++)
                    {
                        bodyPieces[j].AddForce(bodyPieces[j].mass * distanceCorrectionScale * d);
                    }
                }
            }
        }

        protected virtual void ChangeDirection()
        {
            CurrentOrientation = (HorizontalOrientation)(-(int)CurrentOrientation);

            Vector3 s;

            for (int i = 0; i < numBodyPieces; i++)
            {
                positions[i] = bodyPieces[i].position;
            }
            for (int i = 0; i < numBodyPieces; i++)
            {
                bodyPieces[i].position = positions[numBodyPieces - i - 1];
                s = bodyPieces[i].transform.localScale;
                s.x *= -1;
                bodyPieces[i].transform.localScale = s;
            }
        }

        protected void UpdateRotation()
        {
            for (int i = 0; i < numBodyPieces; i++)
            {
                bodyPieces[i].transform
                    .RotateTowardsMovementDirection(FacingRight, (int)CurrentOrientation * groundDirections[i], 
                    movementOptions);
            }
        }

        protected void UpdateGroundDirections()
        {
            for (int i = 0; i < numBodyPieces; i++)
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
            var o = new Vector2(body.position.x + 0.05f, body.position.y);
            return Physics2D.Raycast(o, -Vector2.up, Mathf.Infinity, groundLayer);
        }

        protected RaycastHit2D LeftGroundcast(Rigidbody2D body)
        {
            var o = new Vector2(body.position.x - 0.05f, body.position.y);
            return Physics2D.Raycast(o, -Vector2.up, Mathf.Infinity, groundLayer);
        }
    }

}