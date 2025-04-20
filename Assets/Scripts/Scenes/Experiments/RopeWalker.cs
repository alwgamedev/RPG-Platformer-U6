using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class RopeWalker : MonoBehaviour
    {
        //[SerializeField] Rigidbody2D head;
        //[SerializeField] Collider2D headCollider;

        [SerializeField] protected Rigidbody2D[] bodyPieces;
        [SerializeField] protected MovementOptions movementOptions;
        [SerializeField] protected float maxSpeed;
        [SerializeField] protected float distanceCorrectionForce;

        protected int groundLayer;
        float[] goalDistances;

        protected Vector2[] positions;
        protected Vector2[] groundDirections;//(always pointing left to right)

        public int n { get; private set; }
        public HorizontalOrientation CurrentOrientation { get; protected set; } = HorizontalOrientation.right;
        public bool FacingRight => CurrentOrientation == HorizontalOrientation.right;
        //public Rigidbody2D[] BodyPieces => bodyPieces;
        public float Length { get; private set; }

        protected virtual void Awake()
        {
            n = bodyPieces.Length;
            Debug.Log(n);
            goalDistances = new float[n - 1];
            positions = new Vector2[n];
            groundDirections = new Vector2[n];
            Length = 0;
            for (int i = 0; i < n - 1; i++)
            {
                goalDistances[i] = Vector2.Distance(bodyPieces[i].position, bodyPieces[i + 1].position);
                Length += goalDistances[i];
            }
            groundLayer = LayerMask.GetMask("Ground");
        }

        protected virtual void FixedUpdate()
        {
            UpdatePhysicsData();

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (FacingRight)
                {
                    ChangeDirection();
                }
                HandleMoveInput(-1);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                if (!FacingRight)
                {
                    ChangeDirection();
                }
                HandleMoveInput(1);
            }
        }

        protected virtual void UpdatePhysicsData()
        {
            UpdateGroundDirections();
            UpdateRotation();
        }

        protected virtual void HandleMoveInput(float moveInput)
        {
            Rigidbody2D rb;

            for (int i = 0; i < n; i++)
            {
                rb = bodyPieces[i];
                rb.Move(FacingRight, rb.linearVelocity, moveInput * groundDirections[i], 
                    maxSpeed, movementOptions);
            }

            EnforceDistances();
        }

        protected void EnforceDistances()
        {
            Vector2 d;

            for (int i = 0; i < n - 1; i++)
            {
                d = (bodyPieces[i + 1].position - bodyPieces[i].position).normalized;
                d = bodyPieces[i].position - Mathf.Sign(d.x * (int)CurrentOrientation) * goalDistances[i] * d;
                    //^goal position for bodyPieces[i + 1]
                d = bodyPieces[i + 1].mass * distanceCorrectionForce * (d - bodyPieces[i + 1].position);
                    //^correction force to be applied
                bodyPieces[i + 1].AddForce(d);
            }
        }

        protected virtual void ChangeDirection()
        {
            CurrentOrientation = (HorizontalOrientation)(-(int)CurrentOrientation);

            Vector3 s;

            for (int i = 0; i < n; i++)
            {
                positions[i] = bodyPieces[i].position;
            }
            for (int i = 0; i < n; i++)
            {
                bodyPieces[i].position = positions[n - i - 1];
                s = bodyPieces[i].transform.localScale;
                s.x *= -1;
                bodyPieces[i].transform.localScale = s;
            }
        }

        protected void UpdateRotation()
        {
            for (int i = 0; i < n; i++)
            {
                bodyPieces[i].transform
                    .RotateTowardsMovementDirection(FacingRight, (int)CurrentOrientation * groundDirections[i], 
                    movementOptions);
            }
        }

        protected void UpdateGroundDirections()
        {
            for (int i = 0; i < n; i++)
            {
                groundDirections[i] = GroundDirectionVector(bodyPieces[i]);
            }
        }

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