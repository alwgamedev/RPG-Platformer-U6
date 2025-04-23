using RPGPlatformer.Core;
using System;
using UnityEngine;

namespace RPGPlatformer.Movement
{
    [RequireComponent(typeof(AnimationControl))]
    public class PillBugMovementController : StateDrivenController<PillBugMovementStateManager,
        PillBugMovementStateGraph, PillBugMovementStateMachine, PillBugMover>, 
        IAIMovementController, IInputDependent
    {
        Action OnUpdate;
        Action OnFixedUpdate;

        Vector2 moveInput;

        public IInputSource InputSource { get; private set; }
        public Transform CurrentTarget { get; set; }
        public Vector2 MoveInput
        {
            get => moveInput;
            set
            {
                if (value.x > 0)
                {
                    moveInput = Vector2.right;
                }
                else if (value.x < 0)
                {
                    moveInput = Vector2.left;
                }
                else
                {
                    moveInput = Vector2.zero;
                }
            }
        }
        public bool Moving => MoveInput != Vector2.zero;
        public Vector2 RelativeVelocity => stateDriver.BodyPieces[0].linearVelocity;
        public IMover Mover => stateDriver;
        public HorizontalOrientation CurrentOrientation => stateDriver.CurrentOrientation;
        public bool Curled => stateManager.StateMachine.CurrentState == stateManager.StateGraph.curled;

        protected override void Awake()
        {
            base.Awake();

            InitializeInputSource();
        }

        protected override void Start()
        {
            base.Start();

            OnUpdate = AnimateMovement;
            OnFixedUpdate = HandleMoveInput;
        }

        private void Update()
        {
            //for testing:
            //if (Input.GetKeyDown(KeyCode.Z))
            //    {
            //        SetCurled(!Curled);
            //    }

            //if (Input.GetKeyDown(KeyCode.LeftArrow))
            //{
            //    MoveInput = Vector2.left;
            //}
            //else if (Input.GetKeyDown(KeyCode.RightArrow))
            //{
            //    MoveInput = Vector2.right;
            //}
            //else if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            //{
            //    MoveInput = Vector2.zero;
            //}

            OnUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        public void InitializeInputSource()
        {
            InputSource = GetComponent<IInputSource>();
        }

        protected override void InitializeStateManager()
        {
            stateManager = new(stateDriver, GetComponent<AnimationControl>());
        }

        //protected override void ConfigureStateManager()
        //{
        //    base.ConfigureStateManager();
        //}

        private void HandleMoveInput()
        {
            stateDriver.HandleMoveInput(MoveInput.x);
        }

        private void AnimateMovement()
        {
            if (!Curled)
            {
                stateManager.AnimateMovement(SpeedFraction(stateDriver.RunSpeed));
            }
        }

        //BASIC FUNCTIONS

        public void SetRunning(bool val)
        {
            stateDriver.Running = val;
        }

        public void SetCurled(bool val)
            //expose this at the top level, bc higher up AI controllers will only know about the MovementController
        {
            stateDriver.SetCurled(val);
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
                stateDriver.SetOrientation((HorizontalOrientation)Math.Sign(d));
            }
        }

        public void HardStop(bool maintainVerticalVelocity = true)
        {
            SoftStop();
            stateDriver.Stop();
        }

        public void SoftStop()
        {
            MoveInput = Vector2.zero;
        }

        public void MoveAwayFrom(Vector2 point)
        {
            MoveInput = new(transform.position.x - point.x, 0);
        }

        public void MoveTowards(Vector2 point)
        {
            MoveInput = new(point.x - transform.position.x, 0);
        }

        private float SpeedFraction(float maxSpeed)
        {
            return RelativeVelocity.magnitude / maxSpeed;
        }


        //DEATH AND DISABLE HANDLERS

        public void OnDeath()
        {
            stateManager.Freeze();
            OnUpdate = null;
            OnFixedUpdate = null;
            SoftStop();
        }

        public void OnRevival() { }

        public void OnInputEnabled() { }

        public void OnInputDisabled()
        {
            SoftStop();
        }

        private void OnDestroy()
        {
            OnUpdate = null;
            OnFixedUpdate = null;
        }


        //UNUSED INTERFACE ITEMS

        public bool Jumping => false;
        public bool Freefalling => false;
        public IMountableEntity CurrentMount => null;

        public bool DropOffAhead(HorizontalOrientation direction, out float distance)
        {
            distance = 0;
            return false;
        }

        public void Mount(IMountableEntity entity) { }

        public void Dismount() { }
    }
}