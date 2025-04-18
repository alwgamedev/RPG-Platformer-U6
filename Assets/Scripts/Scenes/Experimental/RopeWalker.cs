using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class RopeWalker : MonoBehaviour
    {
        //[SerializeField] Rigidbody2D head;
        //[SerializeField] Collider2D headCollider;

        [SerializeField] Rigidbody2D[] bodyPieces;
        [SerializeField] MovementOptions movementOptions;
        //[SerializeField] float moveForce;
        [SerializeField] float maxSpeed;
        [SerializeField] float distanceCorrectionForce;
        //[SerializeField] float distanceTolerance;
        //[SerializeField] float goalDistance;

        int n;
        int groundLayer;
        //Collider2D[] bodyColliders;
        float[] goalDistances;
        //bool facingRight = true;

        //Vector2 colliderCenterRight => 
        //    new Vector2(headCollider.bounds.center.x + 0.01f, headCollider.bounds.center.y);
        //Vector2 colliderCenterLeft => new Vector2(headCollider.bounds.center.x - 0.01f, headCollider.bounds.center.y);

        private void Start()
        {
            n = bodyPieces.Length;
            //bodyColliders = new Collider2D[n];
            goalDistances = new float[n - 1];
            //for (int i = 0; i < n; i++)
            //{
            //    bodyColliders[i] = bodyPieces[i].gameObject.GetComponent<Collider2D>();
            //}
            for (int i = 0; i < n - 1; i++)
            {
                goalDistances[i] = Vector2.Distance(bodyPieces[i].position, bodyPieces[i + 1].position);
            }
            groundLayer = LayerMask.GetMask("Ground");
        }

        private void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                //if (facingRight)
                //{
                //    Flip();
                //    facingRight = false;
                //}
                Move(HorizontalOrientation.left);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                //if (!facingRight)
                //{
                //    Flip();
                //    facingRight = true;
                //}
                Move(HorizontalOrientation.right);


            }
        }

        void EnforceDistances()
        {
            float l;
            Vector2 d;
            Vector2 c;

            for (int i = 0; i < n - 1; i++)
            {
                d = bodyPieces[i].position - bodyPieces[i + 1].position;              
                l = d.magnitude;
                c = (l - goalDistances[i]) * distanceCorrectionForce * d / l;
                bodyPieces[i + 1].AddForce(distanceCorrectionForce * c);
            }
        }

        void Move(HorizontalOrientation o)
        {
            Rigidbody2D rb;
            Vector2 d;

            for (int i = 0; i < n; i++)
            {
                rb = bodyPieces[i];
                d = GroundDirectionVector(rb) * (int)o;
                rb.Move(true, rb.linearVelocity, d, maxSpeed, movementOptions);
            }

            EnforceDistances();
        }

        //void MoveAlongGround(Rigidbody2D body, float force, float maxSpeed)
        //{
        //    if (body.linearVelocity.sqrMagnitude < maxSpeed * maxSpeed)
        //    {
        //        body.AddForce(force * GroundDirectionVector(body));
        //    }
        //}

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

        Vector2 GroundDirectionVector(Rigidbody2D body)
        {
            
            return (RightGroundcast(body).point - LeftGroundcast(body).point).normalized;
        }
    }

}