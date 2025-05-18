using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class Climber : AdvancedMover, IClimber
    {
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
            myRigidbody.bodyType = RigidbodyType2D.Kinematic;
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
                EndClimb(true);
                return;
            }

            var p = ClimberData.WorldPosition();
            var d = Vector3.Distance(transform.position, p);
            transform.position = Vector3.Lerp(transform.position, p,
                options.PositionLerpRate * Time.deltaTime / d);
            var tUp = ClimberData.localPosition < 0 ? - ClimberData.currentNode.LowerDirection()
                : ClimberData.currentNode.HigherDirection();
            transform.TweenTransformUpTowards(tUp, options.RotationSpeed);
        }

        public void EndClimb(bool triggerFreefall = false)
        {
            ClimberData = default;
            myRigidbody.bodyType = RigidbodyType2D.Dynamic;
            transform.rotation = Quaternion.identity;
            EnableCollisionWithClimbables(true);

            if (triggerFreefall)
            {
                TriggerFreefall();
            }
        }

        public void EnableCollisionWithClimbables(bool val)
        {
            if (val)
            {
                myCollider.excludeLayers = myCollider.excludeLayers & ~climbableObjectLayer;
            }
            else
            {
                myCollider.excludeLayers = myCollider.excludeLayers | climbableObjectLayer;
            }

            //ClimbableCollisionEnabled = val;
        }
    }
}