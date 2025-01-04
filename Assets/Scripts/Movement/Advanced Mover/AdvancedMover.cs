using UnityEngine;

namespace RPGPlatformer.Movement
{
    public class AdvancedMover : Mover
    {
        [SerializeField] float acceleration = 30;
        [SerializeField] float airborneAccelerationFactor = 0.8f;
        [SerializeField] float runSpeed = 3;
        [SerializeField] float walkSpeed = 0.8f;
        [SerializeField] int maxNumJumps = 2;
        [SerializeField] float jumpForce = 375;
        //[SerializeField] Vector2 defaultJumpForce = 300 * Vector2.up;
        //[SerializeField] Vector2 doubleJumpForce = 500 * Vector2.up;

        protected int currentJumpNum = 0;
        protected float maxSpeed;
        protected bool running;

        public float MaxSpeed => maxSpeed;
        public float RunSpeed => runSpeed;
        public float WalkSpeed => walkSpeed;
        public bool Running
        {
            get => running;
            set
            {
                running = value;
                maxSpeed = running ? runSpeed : walkSpeed;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            maxSpeed = walkSpeed;
        }

        public void MoveGrounded()
        {
            Move(acceleration, maxSpeed, GroundDirectionVector(), false);
        }

        public void MoveAirborne(HorizontalOrientation orientation)
        {
            Move(acceleration * airborneAccelerationFactor, maxSpeed, Vector2.right * (int)orientation, true);
        }

        public float SpeedFraction()
        {
            float speed = Mathf.Abs(myRigidbody.linearVelocity.x);
            return Mathf.Clamp(speed / runSpeed, 0, 1);
        }

        public void Jump()
        {
            if (currentJumpNum < maxNumJumps)
            {
                Jump(JumpForce());
                currentJumpNum++;
            }
        }

        private Vector2 JumpForce()
        {
            if(currentJumpNum == 0)
            {
                return jumpForce * Vector2.up;
            }
            return (0.25f * SpeedFraction() * jumpForce) * Vector2.right + (1.35f * jumpForce) * Vector2.up;
            //Vector2 force = currentJumpNum > 0 ? doubleJumpForce : defaultJumpForce;
            //Vector2 adjustedForce = new Vector2(SpeedFraction() * force.x, force.y);
            //force = adjustedForce.normalized * force.magnitude;
            //return force;
            //jump farther forward when moving faster,
            //but jump higher when still.
        }

        public void ResetJumpNum()
        {
            currentJumpNum = 0;
        }

        public void ToggleRun()
        {
            Running = !Running;
        }
    }
}