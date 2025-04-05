using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    using static PhysicsTools;

    [RequireComponent(typeof(AnimationControl))]
    public abstract class GenericMovementController<T0, T1, T2, T3> : 
        StateDrivenController<T3, T1, T2, T0>, IMovementController
        where T0 : Mover
        where T1 : MovementStateGraph
        where T2 : MovementStateMachine<T1>
        where T3 : MovementStateManager<T1, T2, T0>
    {
        [SerializeField] MovementOptions[] movementOptions;

        //protected T0 mover;
        //protected T3 movementManager;

        protected bool ignoreMoveInputNextUpdate;

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
                    return stateDriver.Rigidbody.linearVelocity - CurrentMount.Velocity;
                }
                return stateDriver.Rigidbody.linearVelocity;
            }
        }
        public HorizontalOrientation CurrentOrientation => stateDriver.CurrentOrientation;
        public IMover Mover => stateDriver;
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
        public bool Grounded => stateManager.StateMachine.CurrentState == stateManager.StateGraph.grounded;
        public bool Freefalling => stateManager.StateMachine.CurrentState == stateManager.StateGraph.freefall;
        public virtual bool Jumping => false;

        protected override void Awake()
        {
            //InitializeMover();
            base.Awake();

            BuildMovementOptionsDictionary();
        }

        protected override void Start()
        {
            base.Start();
            //InitializeMovementManager();
            //ConfigureMovementManager();

            //do these in start in case we want to subscribe functions from components
            InitializeUpdate();
            InitializeFixedUpdate();

            stateDriver.FreefallVerified += OnFreefallVerified;
        }

        protected virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        protected virtual void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();

            //only apply when grounded b/c you want normal physics to apply when jumping
            //(the tiny amount the mount gravity screws with you before your jump gets you far enough to trigger
            //dismount is enough to completely ruin the jump or prevent dismount altogether)
            if (CurrentMount != null && Grounded)
            {
                stateDriver.Accelerate(CurrentMount.LocalGravity);
            }

            ignoreMoveInputNextUpdate = false;
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

        protected override void InitializeStateManager()
        {
            stateManager = (T3)Activator.CreateInstance(typeof(T3), stateDriver, GetComponent<AnimationControl>());
        }

        protected override void ConfigureStateManager()
        {
            //movementManager.Configure();
            base.ConfigureStateManager();

            //movementManager.StateGraph.freefall.OnEntry += OnFreefallEntry;
            stateManager.StateGraph.freefall.OnExit += OnFreefallExit;

            stateManager.StateMachine.StateChange += UpdateMoveOptions;
        }

        //protected virtual void InitializeMover()
        //{
        //    mover = GetComponent<T0>();
        //}


        //BASIC FUNCTIONS

        protected abstract void UpdateMover();

        protected virtual void UpdateMoveOptions(State state)
        {
            //if (state == movementManager.StateGraph.freefall) return;

            currentMovementOptions = GetMovementOptions[state.name];
            GetMoveDirection = MoveDirectionFunction(currentMovementOptions.MoveDirection);
        }

        protected virtual Func<Vector2, Vector2> MoveDirectionFunction(MoveDirection d)
        {
            return d switch
            {
                MoveDirection.Ground => v => stateDriver.GroundDirectionVector(),
                MoveDirection.Input => MoveInputDirection,
                MoveDirection.Horizontal => v => OrientationDirection(),
                MoveDirection.Vertical => v => VerticalOrientationDirection(),
                _ => v => default
            };
        }

        public virtual void MoveTowards(Vector2 point)
        {
            MoveInput = new(point.x - transform.position.x, 0);
        }

        public virtual void MoveAwayFrom(Vector2 point)
        {
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

        public virtual void SetOrientation(Vector2 input)
        {
            SetOrientation(input, currentMovementOptions.FlipSprite, currentMovementOptions.ChangeDirectionWrtGlobalUp);
        }

        public virtual void SetOrientation(Vector2 input, bool updateDirectionFaced, bool flipWrtGlobalUp) 
        {
            if (input.x == 0) return;
            stateDriver.SetOrientation((HorizontalOrientation)Mathf.Sign(input.x), updateDirectionFaced, flipWrtGlobalUp);
        }

        public virtual void SoftStop()
        {
            //use moveInput rather than MoveInput so that
            //MoveInput.set can reference SoftStop without stack overflow
            moveInput = Vector2.zero;
        }

        public virtual void HardStop(bool maintainVerticalVelocity = true)
        {
            stateDriver.Stop(maintainVerticalVelocity);
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

            stateDriver.SetGravityScale(0);
        }

        public virtual void Dismount()
        {
            if (CurrentMount == null) return;

            stateDriver.ReturnGravityScaleToDefault();

            CurrentMount.DirectionChanged -= OnMountDirectionChanged;
            CurrentMount.Destroyed -= Dismount;
            CurrentMount = null;
        }

        protected virtual void OnMountDirectionChanged(HorizontalOrientation o)
        {
            stateDriver.SetOrientation((HorizontalOrientation)(-(int)CurrentOrientation), currentMovementOptions.FlipSprite,
                currentMovementOptions.ChangeDirectionWrtGlobalUp);
            var dp = transform.position - CurrentMount.Position;
            dp = ReflectAlongUnitVector(CurrentMount.VelocitySourceTransformRight, dp);
            transform.position = CurrentMount.Position + dp;
        }

        //STATE CHANGE HANDLERS

        protected virtual void HandleMoveInput()
        {
            HandleMoveInput(MoveInput, Move);
        }

        protected virtual void HandleMoveInput(Vector2 moveInput, Action<Vector2> moveAction)
        {
            if (moveInput != Vector2.zero)
            {
                SetOrientation(moveInput);
                if (!ignoreMoveInputNextUpdate)
                {
                    moveAction(moveInput);
                }
            }
        }

        protected virtual void Move(Vector2 moveInput)
        {
            stateDriver.Move(RelativeVelocity, GetMoveDirection(moveInput), currentMovementOptions);
        }

        protected virtual void MoveWithoutAcceleration(Vector2 moveInput)
        {
            stateDriver.MoveWithoutAcceleration(GetMoveDirection(moveInput), currentMovementOptions);
        }

        protected virtual void OnFreefallVerified()
        {
            if (Freefalling)
            {
                //UpdateMoveOptions(movementManager.StateGraph.freefall);
                stateManager.AnimateFreefall();
            }
        }

        protected virtual void IgnoreMoveInputNextUpdate()
        {
            ignoreMoveInputNextUpdate = true;
        }

        protected void OnFreefallExit()
        {
            stateDriver.UpdateDirectionFaced(currentMovementOptions.ChangeDirectionWrtGlobalUp);
        }


        //DEATH AND DESTROY HANDLERS

        public virtual void OnDeath()
        {
            Dismount();
            TempFixedUpdate = OnFixedUpdate;
            TempUpdate = OnUpdate;
            OnFixedUpdate = null;
            OnUpdate = null;
            moveInput = Vector2.zero;
            stateManager.Freeze();
            stateDriver.OnDeath();
        }

        public virtual void OnRevival()
        {
            stateManager.Unfreeze();
            stateDriver.OnRevival();
            OnFixedUpdate = TempFixedUpdate;
            OnUpdate = TempUpdate;
        }

        protected virtual void OnDestroy()
        {
            GetMoveDirection = null;
            OnUpdate = null;
            OnFixedUpdate = null;
            TempFixedUpdate = null;
            TempUpdate = null;
        }
    }
}