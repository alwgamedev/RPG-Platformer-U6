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
        [SerializeField] Transform leftBound;
        [SerializeField] Transform rightBound;

        Action OnUpdate;
        Action OnFixedUpdate;
        Action TempUpdate;
        Action TempFixedUpdate;

        Vector3 moveInput;

        public IInputSource InputSource { get; private set; }
        public Transform CurrentTarget { get; set; }
        public Vector3 MoveInput => moveInput;
        public bool Moving => (Vector2)MoveInput != Vector2.zero;
        public Vector2 RelativeVelocity => stateDriver.BodyPieces[0].linearVelocity;
        public IMover Mover => stateDriver;
        public HorizontalOrientation CurrentOrientation => stateDriver.CurrentOrientation;
        public bool Curled => stateManager.StateMachine.CurrentState == stateManager.StateGraph.curled;
        public Transform LeftMovementBound
        {
            get => leftBound;
            set => leftBound = value;
        }

        public Transform RightMovementBound
        {
            get => rightBound;
            set => rightBound = value;
        }

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

        //public void SetMoveInput(Vector3 moveInput)
        //{
        //    MoveInput = moveInput;
        //}

        private void HandleMoveInput()
        {
            if (CanMove(moveInput))
            {
                stateDriver.HandleMoveInput(MoveInput);
            }
            else if (Curled)
            {
                SetCurled(false);
            }
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
            if (val && !CanMove(moveInput, .75f)) return;//prevent him from curling and uncurling when at edge of movement bounds
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
            moveInput = Vector3.zero;
            if (Curled)
            {
                SetCurled(false);
            }
        }

        public void MoveAwayFrom(Vector2 point)
        {
            moveInput = new(transform.position.x - point.x, 0, -1);
        }

        public void MoveTowards(Vector2 point)
        {
            moveInput = new(point.x - transform.position.x, 0, 0);
        }

        private float SpeedFraction(float maxSpeed)
        {
            return RelativeVelocity.magnitude / maxSpeed;
        }

        //(for interface)
        public bool CanMove(Vector3 moveInput)
        {
            return CanMove(moveInput, 0);
        }

        private bool CanMove(Vector3 moveInput, float buffer = 0)
        {
            if (rightBound && moveInput.x > 0 && transform.position.x > rightBound.position.x - buffer) return false;
            if (leftBound && moveInput.x < 0 && transform.position.x < leftBound.position.x + buffer) return false;

            return true;
        }


        //DEATH AND DISABLE HANDLERS

        public void OnDeath()
        {
            SetCurled(false);
            //foreach (var b in stateDriver.BodyPieces)
            //{
            //    b.SetKinematic();
            //}
            stateManager.Freeze();
            TempUpdate = OnUpdate;
            TempFixedUpdate = OnFixedUpdate;
            OnUpdate = null;
            OnFixedUpdate = null;
            SoftStop();
        }

        public void OnRevival()
        {
            //foreach (var b in stateDriver.BodyPieces)
            //{
            //    b.bodyType = RigidbodyType2D.Dynamic;
            //}
            stateManager.Unfreeze();
            //stateDriver.OnRevival();
            OnFixedUpdate = TempFixedUpdate;
            OnUpdate = TempUpdate;
        }

        public void OnInputEnabled() { }

        public void OnInputDisabled()
        {
            SoftStop();
        }

        private void OnDestroy()
        {
            OnUpdate = null;
            OnFixedUpdate = null;
            TempUpdate = null;
            TempFixedUpdate = null;
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