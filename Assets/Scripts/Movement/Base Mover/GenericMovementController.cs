using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGPlatformer.Core;

namespace RPGPlatformer.Movement
{
    using static PhysicsTools;

    [RequireComponent(typeof(AnimationControl))]
    //[RequireComponent(typeof(MonoBehaviorInputConfigurer))]
    public abstract class GenericMovementController<T0, T1, T2, T3> : 
        StateDrivenController<T3, T1, T2, T0>, IMovementController, IInputDependent
        where T0 : Mover
        where T1 : MovementStateGraph
        where T2 : MovementStateMachine<T1>
        where T3 : MovementStateManager<T1, T2, T0>
    {
        [SerializeField] MovementOptions[] movementOptions;
        [SerializeField] bool moveAwayBackwards = true;

        protected bool ignoreMoveInputNextUpdate;
        protected Vector3 moveInput;

        protected Func<Vector3, Vector2> GetMoveDirection = (v) => default;
        protected Action OnFixedUpdate;
        protected Action TempFixedUpdate;
        protected Action OnUpdate;
        protected Action TempUpdate;

        protected MovementOptions currentMovementOptions;
        protected Dictionary<string, MovementOptions> GetMovementOptions = new();

        public IInputSource InputSource { get; protected set; }
        public HorizontalOrientation CurrentOrientation => stateDriver.CurrentOrientation;
        public IMover Mover => stateDriver;
        public virtual Vector3 MoveInput
        //child classes will override get/set
        //z-coord will be used to indicate whether object is backing up (i.e. orientation gets *= -1)
        {
            get => moveInput;
            protected set
            {
                moveInput = value;
            }
        }
        public bool Moving => (Vector2)MoveInput != Vector2.zero;
        public bool Grounded => stateManager.StateMachine.CurrentState == stateManager.StateGraph.grounded;
        public bool Freefalling => stateManager.StateMachine.CurrentState == stateManager.StateGraph.freefall;
        public virtual bool Swimming => stateManager.StateMachine.CurrentState == stateManager.StateGraph.swimming;
        public virtual bool Jumping => false;
        public IMountableEntity CurrentMount { get; protected set; }
        //can be any "ambient velocity source" (e.g. we are on a moving platform)
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

        protected override void Awake()
        {
            base.Awake();

            InitializeInputSource();
            BuildMovementOptionsDictionary();
        }

        protected override void Start()
        {
            base.Start();

            InitializeUpdate();
            InitializeFixedUpdate();

            stateDriver.FreefallVerified += OnFreefallVerified;
            stateDriver.WaterExited += OnWaterExited;

            stateDriver.InitializeState();
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
            foreach (var s in Enum.GetValues(typeof(MovementStates)))
            {
                GetMovementOptions[((MovementStates)s).ToString()]
                    = movementOptions.FirstOrDefault(x => x.MovementType == (MovementStates)s);
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

        protected virtual Func<Vector3, Vector2> MoveDirectionFunction(MoveDirection d)
        {
            return d switch
            {
                MoveDirection.Ground => v => Mathf.Sign(v.z) * stateDriver.GroundDirectionVector(),
                MoveDirection.Input => MoveInputDirection,
                MoveDirection.Horizontal => v => Mathf.Sign(v.z) * OrientationDirection(),
                MoveDirection.Vertical => v => Mathf.Sign(v.z) * VerticalOrientationDirection(),
                _ => v => default
            };
        }

        public abstract void SetRunning(bool val);

        public virtual void MoveTowards(Vector2 point)
        {
            MoveInput = new(point.x - transform.position.x, 0);
        }

        public virtual void MoveAwayFrom(Vector2 point)
        {
            MoveInput = new(transform.position.x - point.x, 0, moveAwayBackwards ? -1 : 0);
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
            SetOrientation((Vector2)(target - transform.position));
        }

        public virtual void SetOrientation(Vector3 input)
        {
            SetOrientation(input, currentMovementOptions.FlipSprite, currentMovementOptions.ChangeDirectionWrtGlobalUp);
        }

        public virtual void SetOrientation(Vector3 input, bool updateDirectionFaced, bool flipWrtGlobalUp) 
        {
            if (input.x == 0) return;

            stateDriver.SetOrientation((HorizontalOrientation)(Mathf.Sign(input.z) * Mathf.Sign(input.x)), 
                updateDirectionFaced, flipWrtGlobalUp);
        }

        public virtual void SoftStop()
        {
            MoveInput = Vector3.zero;
        }

        public virtual void HardStop(bool maintainVerticalVelocity = true)
        {
            stateDriver.Stop(maintainVerticalVelocity);
            SoftStop();
        }

        protected Vector2 MoveInputDirection(Vector3 moveInput)
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
            dp = ReflectAcrossPerpendicularHyperplane(CurrentMount.VelocitySourceTransformRight, dp);
            transform.position = CurrentMount.Position + dp;
        }

        //STATE CHANGE HANDLERS

        protected virtual void HandleMoveInput()
        {
            HandleMoveInput(MoveInput, Move);
        }

        protected virtual void HandleMoveInput(Vector3 moveInput, Action<Vector3> moveAction)
        {
            if ((Vector2)moveInput != Vector2.zero)
            {
                SetOrientation(moveInput);
                if (!ignoreMoveInputNextUpdate)
                {
                    moveAction(moveInput);
                }
            }
        }

        protected virtual void Move(Vector3 moveInput)
        {
            stateDriver.Move(moveInput.z < 0, RelativeVelocity, GetMoveDirection(moveInput), currentMovementOptions);
        }

        protected virtual void MoveWithoutAcceleration(Vector3 moveInput)
        {
            stateDriver.MoveWithoutAcceleration(moveInput.z < 0, GetMoveDirection(moveInput), currentMovementOptions);
        }

        protected virtual void OnFreefallVerified()
        {
            if (Freefalling)
            {
                stateManager.AnimateFreefall();
            }
        }

        protected void OnFreefallExit()
        {
            stateDriver.UpdateDirectionFaced(currentMovementOptions.ChangeDirectionWrtGlobalUp);
        }

        protected virtual void IgnoreMoveInputNextUpdate()
        {
            ignoreMoveInputNextUpdate = true;
        }

        protected virtual void OnWaterExited()
        {
            stateDriver.HandleWaterExit(Swimming);
        }


        //INPUT SYSTEM

        public void InitializeInputSource()
        {
            InputSource = GetComponent<IInputSource>();
        }

        public void OnInputEnabled() { }

        public void OnInputDisabled()
        {
            SoftStop();
            //and I guess idea is that MoveInput will not be set again while input disabled,
            //so we don't have to set (Fixed)Update null?
        }


        //DEATH AND DESTROY HANDLERS

        public virtual void OnDeath()
        {
            Dismount();
            TempFixedUpdate = OnFixedUpdate;
            TempUpdate = OnUpdate;
            OnFixedUpdate = null;
            OnUpdate = null;
            SoftStop();
            stateManager.Freeze();
            stateDriver.PlayDeathEffect();
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