using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    [RequireComponent(typeof(AnimationControl))]
    public abstract class GenericMovementController<T0, T1, T2, T3> : MonoBehaviour
        where T0 : Mover
        where T1 : MovementStateGraph
        where T2 : MovementStateMachine<T1>
        where T3 : MovementStateManager<T1, T2, T0>
    {
        [SerializeField] MovementOptions[] movementOptions;
        [SerializeField] protected bool matchRotationToGround;

        protected T0 mover;
        protected T3 movementManager;

        protected Func<Vector2> GetMoveDirection = () => default;
        //protected Action<Vector2> CurrentMoveAction;
        protected Action OnFixedUpdate;
        protected Action TempFixedUpdate;
        protected Action OnUpdate;
        protected Action TempUpdate;

        protected Vector2 moveInput;

        protected MovementOptions currentMovementOptions;
        protected Dictionary<string, MovementOptions> GetMovementOptions = new();

        public bool Moving => moveInput != Vector2.zero;
        public Rigidbody2D Rigidbody => mover.Rigidbody;
        public HorizontalOrientation CurrentOrientation => mover.CurrentOrientation;
        public IMover Mover => mover;
        public virtual Vector2 MoveInput
        //child classes may want to override get/set
        {
            get => moveInput;
            set
            {
                moveInput = value;
            }
        }

        public bool Grounded => movementManager.StateMachine.CurrentState == movementManager.StateGraph.grounded;

        public bool Freefalling => movementManager.StateMachine.CurrentState == movementManager.StateGraph.freefall;

        protected virtual void Awake()
        {
            InitializeMover();

            BuildMovementOptionsDictionary();

            OnFixedUpdate += HandleMoveInput;
        }

        protected virtual void Start()
        {
            InitializeMovementManager();
            ConfigureMovementManager();

            mover.FreefallVerified += OnFreefallVerified;
        }

        protected virtual void Update()
        {
            UpdateMover();
            OnUpdate?.Invoke();
        }

        protected virtual void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        protected virtual void BuildMovementOptionsDictionary()
        {
            foreach (var type in Enum.GetValues(typeof(MovementType)))
            {
                GetMovementOptions[((MovementType)type).ToString()]
                    = movementOptions.FirstOrDefault(x => x.MovementType == (MovementType)type);
            }
        }

        protected virtual void InitializeMovementManager()
        {
            movementManager = (T3)Activator.CreateInstance(typeof(T3), mover, GetComponent<AnimationControl>());
        }

        protected virtual void ConfigureMovementManager()
        {
            movementManager.Configure();

            //movementManager.StateGraph.grounded.OnEntry += OnGroundedEntry;
            movementManager.StateGraph.freefall.OnEntry += OnFreefallEntry;
            movementManager.StateGraph.freefall.OnExit += OnFreefallExit;

            movementManager.StateMachine.StateChange += UpdateMoveOptions;
        }

        protected virtual void InitializeMover()
        {
            mover = GetComponent<T0>();
        }


        //BASIC FUNCTIONS

        protected abstract void UpdateMover();

        protected virtual void UpdateMoveOptions(State state)
        {
            if (state == movementManager.StateGraph.freefall) return;

            currentMovementOptions = GetMovementOptions[state.name];
            GetMoveDirection = MoveDirectionFunction(currentMovementOptions.MoveDirection);
        }

        protected virtual Func<Vector2> MoveDirectionFunction(MoveDirection d)
        {
            return d switch
            {
                MoveDirection.Ground => mover.GroundDirectionVector,
                MoveDirection.Input => MoveInputDirection,
                MoveDirection.Horizontal => OrientationDirection,
                MoveDirection.Vertical => VerticalOrientationDirection,
                _ => () => default
            };
        }

        public virtual void MoveTowards(Vector2 point)
        {
            Vector2 inp = new(point.x - transform.position.x, 0);
            MoveInput = inp;
        }

        public virtual void MoveAwayFrom(Vector2 point)
        {
            if (transform.position.x == point.x)
            {
                MoveInput = Vector2.right;
                return;
            }
            Vector2 inp = new(transform.position.x - point.x, 0);
            MoveInput = inp;
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
            SetOrientation(target - transform.position);
        }

        public virtual void SetOrientation(Vector2 input, bool updateDirectionFaced = true)
        {
            if (input.x == 0) return;
            mover.SetOrientation((HorizontalOrientation)Mathf.Sign(input.x), updateDirectionFaced);
        }

        public virtual void SoftStop()
        {
            //use moveInput rather than MoveInput so that
            //MoveInput.set can reference SoftStop without stack overflow
            moveInput = Vector2.zero;
        }

        public virtual void HardStop()
        {
            mover.Stop();
            SoftStop();
        }

        protected Vector2 MoveInputDirection()
        {
            return moveInput.normalized;
        }

        protected Vector2 OrientationDirection()
        {
            return (int)CurrentOrientation * Vector2.right;
        }

        protected Vector2 VerticalOrientationDirection()
        {
            return (int)CurrentOrientation * Vector2.up;
        }


        //STATE CHANGE HANDLERS

        protected virtual void HandleMoveInput()
        {
            if (moveInput != Vector2.zero)
            {
                //CurrentMoveAction?.Invoke(moveInput);
                SetOrientation(moveInput, currentMovementOptions.FlipSprite);
                mover.Move(GetMoveDirection(), currentMovementOptions);
            }
        }

        //protected virtual void OnGroundedEntry()
        //{
        //    CurrentMoveAction = GroundedMoveAction;
        //}

        protected virtual void OnFreefallEntry() { }

        protected virtual void OnFreefallVerified()
        {
            if (Freefalling)
            {
                //CurrentMoveAction = FreefallMoveAction;
                UpdateMoveOptions(movementManager.StateGraph.freefall);
                movementManager.AnimateFreefall();
            }
        }

        protected void OnFreefallExit()
        {
            mover.UpdateDirectionFaced();
        }

        //protected abstract void GroundedMoveAction(Vector2 input);

        //protected abstract void FreefallMoveAction(Vector2 input);


        //DEATH AND DESTROY HANDLERS

        public virtual void OnDeath()
        {
            TempFixedUpdate = OnFixedUpdate;
            TempUpdate = OnUpdate;
            OnFixedUpdate = null;
            OnUpdate = null;
            moveInput = Vector2.zero;
            movementManager.Freeze();
            mover.OnDeath();
        }

        public virtual void OnRevival()
        {
            movementManager.Unfreeze();
            mover.OnRevival();
            OnFixedUpdate = TempFixedUpdate;
            OnUpdate = TempUpdate;
        }

        protected virtual void OnDestroy()
        {
            //CurrentMoveAction = null;
            GetMoveDirection = null;
            OnUpdate = null;
            OnFixedUpdate = null;
        }
    }
}