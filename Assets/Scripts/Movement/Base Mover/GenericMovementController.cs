using System;
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
        [SerializeField] protected bool matchRotationToGround;

        protected T0 mover;
        protected T3 movementManager;

        protected Action<Vector2> CurrentMoveAction;
        protected Action OnFixedUpdate;
        protected Action TempFixedUpdate;
        protected Action OnUpdate;
        protected Action TempUpdate;

        protected Vector2 moveInput;

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

        protected virtual void Awake()
        {
            InitializeMover();

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

        protected virtual void UpdateMover()
        {
            mover.UpdateGroundHits();
            mover.UpdateState(movementManager.StateMachine.HasState(typeof(Jumping)),
                movementManager.StateMachine.HasState(typeof(Freefall)));
        }

        protected virtual void InitializeMovementManager()
        {
            movementManager = (T3)Activator.CreateInstance(typeof(T3), mover, GetComponent<AnimationControl>());
        }

        protected virtual void ConfigureMovementManager()
        {
            movementManager.Configure();

            movementManager.StateGraph.grounded.OnEntry += OnGroundedEntry;
            movementManager.StateGraph.freefall.OnEntry += OnFreefallEntry;
            movementManager.StateGraph.freefall.OnExit += OnFreefallExit;
        }

        protected virtual void InitializeMover()
        {
            mover = GetComponent<T0>();
        }


        //BASIC FUNCTIONS

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


        //STATE CHANGE HANDLERS

        protected virtual void HandleMoveInput()
        {
            if (moveInput != Vector2.zero)
            {
                CurrentMoveAction?.Invoke(moveInput);
            }
        }

        protected virtual void OnGroundedEntry()
        {
            CurrentMoveAction = GroundedMoveAction;
        }

        protected virtual void OnFreefallEntry() { }

        protected virtual void OnFreefallVerified()
        {
            if (movementManager.StateMachine.HasState(typeof(Freefall)))
            {
                CurrentMoveAction = FreefallMoveAction;
                movementManager.AnimateFreefall();
            }
        }

        protected void OnFreefallExit()
        {
            mover.UpdateDirectionFaced();
        }

        protected abstract void GroundedMoveAction(Vector2 input);

        protected abstract void FreefallMoveAction(Vector2 input);


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
            CurrentMoveAction = null;
            OnUpdate = null;
            OnFixedUpdate = null;
        }
    }
}