using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class Climber : AdvancedMover, IClimber
    {
        int climbableObjectLayer;

        protected override void Awake()
        {
            base.Awake();

            climbableObjectLayer = LayerMask.GetMask("Climbable Object");
        }

        public ClimberData ClimberData { get; protected set; }

        public override void Jump(Vector2 force, bool triggerJumping = true)
        {
            if (ClimberData.currentNode != null)
            {
                EndClimb();
            }

            base.Jump(force, triggerJumping);
        }

        //CLIMBING

        //and somewhere we need to trigger climbing state (and in entry, 
        //movement controller will configure its update functions etc.)

        public void TryGrabOntoClimbableObject(ClimbingMovementOptions options)
        {
            Collider2D c = Physics2D.OverlapCircle(transform.position, options.NodeDetectionRadius, 
                climbableObjectLayer);
            if (c && c.TryGetComponent(out ClimbNode node))
            {
                ClimberData = new ClimberData(node, node.WorldToLocalPosition(transform.position));
                Trigger(typeof(Climbing).Name);
            }
        }

        public void OnBeginClimb()
        {
            myRigidbody.bodyType = RigidbodyType2D.Kinematic;
            myCollider.enabled = false;
        }

        public void UpdateClimb(float moveInput, ClimbingMovementOptions options)
        {
            if (moveInput != 0)
            {
                moveInput = Mathf.Sign(moveInput);
            }

            //Debug.Log($"before: {ClimberData.currentNode.name}, {ClimberData.localPosition}");

            if (ClimberData.currentNode)
            {
                ClimberData = ClimberData.currentNode.GetClimberData(
                    ClimberData.localPosition + moveInput * options.ClimbSpeed * Time.deltaTime);
            }

            if (!ClimberData.currentNode)
            {
                EndClimb(true);
                return;
            }

            //Debug.Log($"after: {ClimberData.currentNode.name}, {ClimberData.localPosition}");

            //we could try lerp unclamped also
            var p = ClimberData.WorldPosition();
            var d = Vector3.Distance(transform.position, p);
            transform.position = Vector3.Lerp(transform.position, p,
                options.PositionLerpRate * Time.deltaTime / d);
            //transform.position = ClimberData.WorldPosition();
            var tUp = ClimberData.localPosition < 0 ? - ClimberData.currentNode.LowerDirection()
                : ClimberData.currentNode.HigherDirection();
            transform.TweenTransformUpTowards(tUp, options.RotationSpeed);
        }

        public void EndClimb(bool triggerFreefall = false)
        {
            ClimberData = default;
            myRigidbody.bodyType = RigidbodyType2D.Dynamic;
            myRigidbody.linearVelocity = Vector3.zero;//otherwise rope velocity and collision send you flying
            transform.rotation = Quaternion.identity;
            myCollider.enabled = true;

            if (triggerFreefall)
            {
                TriggerFreefall();
            }
        }
    }
}