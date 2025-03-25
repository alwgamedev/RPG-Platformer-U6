using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    [RequireComponent(typeof(AnimationControl))]
    public abstract class GenericMovementController<T0, T1, T2, T3> : MonoBehaviour, IMovementController
        where T0 : Mover
        where T1 : MovementStateGraph
        where T2 : MovementStateMachine<T1>
        where T3 : MovementStateManager<T1, T2, T0>
    {
        [SerializeField] MovementOptions[] movementOptions;

        protected T0 mover;
        protected T3 movementManager;

        protected Func<Vector2, Vector2> GetMoveDirection = (v) => default;
        protected Action OnFixedUpdate;
        protected Action TempFixedUpdate;
        protected Action OnUpdate;
        protected Action TempUpdate;

        protected Vector2 moveInput;

        protected MovementOptions currentMovementOptions;
        protected Dictionary<string, MovementOptions> GetMovementOptions = new();

        public Vector2 RelativeVelocity
        {
            get
            {
                if (CurrentMount != null)
                {
                    return mover.Rigidbody.linearVelocity - CurrentMount.Velocity;
                }
                return mover.Rigidbody.linearVelocity;
            }
        }
        public HorizontalOrientation CurrentOrientation => mover.CurrentOrientation;
        public IMover Mover => mover;
        public IMountableEntity CurrentMount { get; protected set; }
        //can be any "ambient velocity source" (e.g. we are on a moving platform)
        public virtual Vector2 MoveInput
        //child classes will override get/set
        {
            get => moveInput;
            set
            {
                moveInput = value;
            }
        }
        public bool Moving => moveInput != Vector2.zero;
        public bool Grounded => movementManager.StateMachine.CurrentState == movementManager.StateGraph.grounded;
        public bool Freefalling => movementManager.StateMachine.CurrentState == movementManager.StateGraph.freefall;
        public virtual bool Jumping => false;

        protected virtual void Awake()
        {
            InitializeMover();
            BuildMovementOptionsDictionary();
        }

        protected virtual void Start()
        {
            InitializeMovementManager();
            ConfigureMovementManager();

            //do these in start in case we want to subscribe functions from components
            InitializeUpdate();
            InitializeFixedUpdate();

            mover.FreefallVerified += OnFreefallVerified;
        }

        protected virtual void Update()
        {
            //UpdateMover();
            OnUpdate?.Invoke();
        }

        protected virtual void FixedUpdate()
        {
            //we've been doing update move in update, but would it make more sense to do it in fixed update
            //so we have accurate data before moving?
            //UpdateMover();

            OnFixedUpdate?.Invoke();

            if (CurrentMount != null && Grounded)
            {
                mover.Rigidbody.linearVelocity += Time.deltaTime * CurrentMount.LocalGravity;
            }
        }

        protected virtual void InitializeUpdate()
        {
            OnUpdate += UpdateMover;
        }

        protected virtual void InitializeFixedUpdate()
        {
            OnFixedUpdate += HandleMoveInput;
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

            //movementManager.StateGraph.freefall.OnEntry += OnFreefallEntry;
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

        protected virtual Func<Vector2, Vector2> MoveDirectionFunction(MoveDirection d)
        {
            return d switch
            {
                MoveDirection.Ground => v => mover.GroundDirectionVector(),
                MoveDirection.Input => MoveInputDirection,
                MoveDirection.Horizontal => v => OrientationDirection(),
                MoveDirection.Vertical => v => VerticalOrientationDirection(),
                _ => v => default
            };
        }

        public virtual void MoveTowards(Vector2 point)
        {
            //Vector2 inp = 
            MoveInput = new(point.x - transform.position.x, 0);
        }

        public virtual void MoveAwayFrom(Vector2 point)
        {
            //if (transform.position.x == point.x)
            //{
            //    MoveInput = Vector2.right;
            //    return;
            //}
            //Vector2 inp = new(transform.position.x - point.x, 0);
            MoveInput = new(transform.position.x - point.x, 0);
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

        public virtual void HardStop(bool maintainVerticalVelocity = true)
        {
            mover.Stop(maintainVerticalVelocity);
            SoftStop();
        }

        protected Vector2 MoveInputDirection(Vector2 moveInput)
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

        public virtual float SpeedFraction(float maxSpeed)
        {
            return RelativeVelocity.magnitude / maxSpeed;
        }

        //note: can return negative (which you may want if you want animator to clamp negative to 0)
        //so leaves you the option of whether you want to take abs later
        public virtual float HorizontalSpeedFraction(float maxSpeed)
        {
            return RelativeVelocity.x / maxSpeed;
        }

        public virtual float VerticalSpeedFraction(float maxSpeed)
        {
            return RelativeVelocity.y / maxSpeed;
        }

        public virtual void Mount(IMountableEntity entity)
        {
            if (entity == null || entity == CurrentMount) return;

            Dismount();

            CurrentMount = entity;

            entity.DirectionChanged += OnMountDirectionChanged;
            entity.Destroyed += Dismount;

            mover.SetGravityScale(0);
        }

        public virtual void Dismount()
        {
            if (CurrentMount == null) return;

            CurrentMount.DirectionChanged -= OnMountDirectionChanged;
            CurrentMount.Destroyed -= Dismount;
            CurrentMount = null;

            mover.ReturnGravityScaleToDefault();
        }

        protected virtual void OnMountDirectionChanged(HorizontalOrientation o)
        {
            //flip direction rather than just SetDirection(o)
            //bc e.g. we may have been facing backwards on the mount before
            mover.SetOrientation((HorizontalOrientation)(-(int)CurrentOrientation), currentMovementOptions.FlipSprite);
            var d = Vector2.Dot(transform.position - CurrentMount.Position, CurrentMount.VelocitySourceTransformRight);
            transform.position -= 2 * d * CurrentMount.VelocitySourceTransformRight;
        }


        //STATE CHANGE HANDLERS

        protected virtual void HandleMoveInput()
        {
            HandleMoveInput(MoveInput);
        }

        protected virtual void HandleMoveInput(Vector2 moveInput)
        {
            if (moveInput != Vector2.zero)
            {
                SetOrientation(moveInput, currentMovementOptions.FlipSprite);
                Move(moveInput);
            }
        }
        protected virtual void Move(Vector2 moveInput)
        {
            mover.Move(GetMoveDirection(moveInput), currentMovementOptions);
        }

        //protected virtual void OnFreefallEntry() { }

        protected virtual void OnFreefallVerified()
        {
            if (Freefalling)
            {
                UpdateMoveOptions(movementManager.StateGraph.freefall);
                movementManager.AnimateFreefall();
            }
        }

        protected void OnFreefallExit()
        {
            mover.UpdateDirectionFaced();
        }


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
            GetMoveDirection = null;
            OnUpdate = null;
            OnFixedUpdate = null;
        }
    }
}