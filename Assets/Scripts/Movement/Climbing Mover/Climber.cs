using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class Climber : AdvancedMover, IClimber
    {
        [SerializeField] Transform climbingAnchor;//usually the character's hand

        protected int climbableObjectLayer;

        //public bool ClimbableCollisionEnabled { get; protected set; }

        protected override void Awake()
        {
            base.Awake();

            climbableObjectLayer = LayerMask.GetMask("Climbable Object");
        }

        public ClimberData ClimberData { get; protected set; }

        public override void Jump(Vector2 force, bool triggerJumping = true)
        {
            //necessary to re-enable rigidbody before applying jump force
            //(yes, EndClimb will get called again when you exit climbing state, but oh well)
            if (ClimberData.currentNode != null)
            {
                EndClimb();
            }

            base.Jump(force, triggerJumping);
        }


        //CLIMBING

        public void TryGrabOntoClimbableObject(ClimbingMovementOptions options)
        {
            Collider2D c = Physics2D.OverlapCircle(transform.position, options.NodeDetectionRadius, 
                climbableObjectLayer);
            if (c && c.TryGetComponent(out IClimbNode node))
            {
                ClimberData = new ClimberData(node, node.WorldToLocalPosition(transform.position));
                Trigger(typeof(Climbing).Name);
            }
        }

        public void OnBeginClimb()
        {
            myRigidbody.SetKinematic();
            //myRigidbody.totalForce = Vector2.zero;
            //myRigidbody.linearVelocity = Vector2.zero;
            //myRigidbody.bodyType = RigidbodyType2D.Kinematic;
            EnableCollisionWithClimbables(false);
        }

        public void UpdateClimb(float moveInput, ClimbingMovementOptions options)
        {
            if (moveInput != 0)
            {
                moveInput = Mathf.Sign(moveInput);
            }

            if (ClimberData.currentNode != null)
            {
                ClimberData = ClimberData.currentNode.GetClimberData(
                    ClimberData.localPosition + moveInput * options.ClimbSpeed * Time.deltaTime);
            }

            if (ClimberData.currentNode == null)
            {
                FallOffClimbable();
                return;
            }

            var p = ClimberData.WorldPosition();
            var delta = transform.position - climbingAnchor.position;
            var d = Vector3.Distance(climbingAnchor.position, p);
            transform.position = delta + Vector3.Lerp(climbingAnchor.position, p,
                options.PositionLerpRate * Time.deltaTime / d);
            var tUp = ClimberData.localPosition < 0 ? -ClimberData.currentNode.LowerDirection()
                : ClimberData.currentNode.HigherDirection();
            transform.TweenTransformUpTowards(tUp, options.RotationSpeed);
        }

        public void EndClimb()
        {
            if (ClimberData.currentNode != null)
            {
                myRigidbody.linearVelocity = ClimberData.currentNode.VelocityAtPosition(ClimberData.localPosition);
            }
            ClimberData = default;
            myRigidbody.bodyType = RigidbodyType2D.Dynamic;
            transform.rotation = Quaternion.identity;
        }

        public void FallOffClimbable()
        {
            Debug.Log("fall off climb");
            TriggerFreefall();
        }

        public void EnableCollisionWithClimbables(bool val)
        {
            if (val)
            {
                myCollider.excludeLayers &= /*myCollider.excludeLayers &*/ ~climbableObjectLayer;
            }
            else
            {
                myCollider.excludeLayers |= /*myCollider.excludeLayers |*/ climbableObjectLayer;
            }
        }
    }
}