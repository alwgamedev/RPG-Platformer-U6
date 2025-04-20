using System.Linq;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class RopeWalker : MonoBehaviour
    {
        //[SerializeField] Rigidbody2D head;
        //[SerializeField] Collider2D headCollider;

        [SerializeField] Rigidbody2D[] bodyPieces;
        [SerializeField] MovementOptions movementOptions;
        [SerializeField] float maxSpeed;
        [SerializeField] float distanceCorrectionForce;

        //int n;
        int groundLayer;
        float[] goalDistances;

        Vector2[] positions;
        Vector2[] moveDirections;//(always pointing left to right)
        HorizontalOrientation currentOrientation = HorizontalOrientation.right;

        public int n { get; private set; }
        public bool facingRight => currentOrientation == HorizontalOrientation.right;
        public Rigidbody2D[] BodyPieces => bodyPieces;
        public float Length { get; private set; }

        private void Awake()
        {
            n = bodyPieces.Length;
            goalDistances = new float[n - 1];
            positions = new Vector2[n];
            moveDirections = new Vector2[n];
            Length = 0;
            for (int i = 0; i < n - 1; i++)
            {
                goalDistances[i] = Vector2.Distance(bodyPieces[i].position, bodyPieces[i + 1].position);
                Length += goalDistances[i];
            }
            groundLayer = LayerMask.GetMask("Ground");
        }

        private void FixedUpdate()
        {
            UpdateMoveDirections();
            UpdateRotation();

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (facingRight)
                {
                    ChangeDirection();
                }
                Move(HorizontalOrientation.left);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                if (!facingRight)
                {
                    ChangeDirection();
                }
                Move(HorizontalOrientation.right);
            }

            EnforceDistances();
        }

        void Move(HorizontalOrientation o)
        {
            Rigidbody2D rb;

            for (int i = 0; i < n; i++)
            {
                rb = bodyPieces[i];
                rb.Move(facingRight, rb.linearVelocity, moveDirections[i], 
                    maxSpeed, movementOptions);
            }
        }

        void EnforceDistances()
        {
            Vector2 d;

            for (int i = 0; i < n - 1; i++)
            {
                d = (bodyPieces[i + 1].position - bodyPieces[i].position).normalized;
                d = bodyPieces[i].position - Mathf.Sign(d.x * (int)currentOrientation) * goalDistances[i] * d;
                    //^goal position for bodyPieces[i + 1]
                d = bodyPieces[i + 1].mass * distanceCorrectionForce * (d - bodyPieces[i + 1].position);
                    //^correction force to be applied
                bodyPieces[i + 1].AddForce(d);
            }
        }

        void ChangeDirection()
        {
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

            currentOrientation = (HorizontalOrientation)(-(int)currentOrientation);
        }

        private void UpdateRotation()
        {
            for (int i = 0; i < n; i++)
            {
                bodyPieces[i].transform
                    .RotateTowardsMovementDirection(facingRight, moveDirections[i], movementOptions);
            }
        }

        private void UpdateMoveDirections()
        {
            for (int i = 0; i < n; i++)
            {
                moveDirections[i] = ((int)currentOrientation) * GroundDirectionVector(bodyPieces[i]);
            }
        }

        Vector2 GroundDirectionVector(Rigidbody2D body)
        {

            return (RightGroundcast(body).point - LeftGroundcast(body).point).normalized;
        }

        RaycastHit2D RightGroundcast(Rigidbody2D body)
        {
            var o = new Vector2(body.position.x + 0.05f, body.position.y);
            return Physics2D.Raycast(o, -Vector2.up, Mathf.Infinity, groundLayer);
        }

        RaycastHit2D LeftGroundcast(Rigidbody2D body)
        {
            var o = new Vector2(body.position.x - 0.05f, body.position.y);
            return Physics2D.Raycast(o, -Vector2.up, Mathf.Infinity, groundLayer);
        }
    }

}