using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using System.Threading;
using RPGPlatformer.SceneManagement;


namespace RPGPlatformer.Combat
{
    [RequireComponent(typeof(TickTimer))]
    [RequireComponent(typeof(AnimationControl))]
    [RequireComponent(typeof(Combatant))]
    [RequireComponent(typeof(MonoBehaviourPauseConfigurer))]
    public class CombatController : MonoBehaviour, ICombatController, IAbilityBarOwner, IPausable
    {
        [SerializeField] protected float timeToLeaveCombat = 200;
        [SerializeField] protected List<SerializedUnarmedAbilityBarItem> unarmedAbilityBarItems;
        [SerializeField] protected List<SerializedMageAbilityBarItem> mageAbilityBarItems;
        [SerializeField] protected List<SerializedMeleeAbilityBarItem> meleeAbilityBarItems;
        [SerializeField] protected List<SerializedRangedAbilityBarItem> rangedAbilityBarItems;
        [SerializeField] protected bool useDefaultAbilityBars = true;
        [SerializeField] protected bool immuneToStuns = false;

        protected Combatant combatant;
        protected CombatStateManager combatManager;

        protected AbilityBar currentAbilityBar = new();
        protected Dictionary<CombatStyle, AbilityBar> GetAbilityBar = new();
        protected AbilityBar unarmedAbilityBar = new();
        protected AbilityBar mageAbilityBar = new();
        protected AbilityBar meleeAbilityBar = new();
        protected AbilityBar rangedAbilityBar = new();

        protected TickTimer tickTimer;
        protected AttackAbility queuedAbility;
        protected bool postCancellationLock;
        //post-cancellation lock means that if fire button is still down after cancelling a channeled ability,
        //it will wait until you release fire button to fully end channel
        //instead of immediately going back to auto-attacking

        protected bool aimingActionInUse;
        protected Action OnLateUpdate;
        protected Action CurrentAimingAction;
        protected Action StoredAction;
        //intended to be used with animation events;
        //cleared in EndChannel, should not be used with async abilities
        //(think I could adjust it to work, but rather keep things simple and reliable for now)

        protected List<(float, bool)> activeStuns = new();

        public AbilityBar CurrentAbilityBar => currentAbilityBar;
        public TickTimer TickTimer => tickTimer;
        public bool GlobalCooldown { get; protected set; }
        public bool ChannelingAbility { get; protected set; }
        public bool PoweringUp { get; protected set; }
        public bool FireButtonIsDown { get; protected set; }
        public ICombatant Combatant => combatant;
        public IMovementController MovementController { get; protected set; }
        public virtual IInputSource InputSource { get; protected set; }

        public event Action CombatEntered;
        public event Action CombatExited;
        public event Action AbilityBarResetEvent;
        public event Action<AttackAbility> OnCooldownStarted;
        public event Action OnFireButtonDown;
        public event Action OnFireButtonUp;
        public event Action OnChannelStarted;
        public event Action OnChannelEnded;
        public event Action OnPowerUpStarted;
        public event Action OnPowerUpEnded;
        public event Action OnMaximumPowerAchieved;
        public event Action OnDeath;
        public event Action OnRevive;
        public event Action<float> HealthChangeEffected;
            //^note parameter represents damage (so positive is damage taken, and negative is health gained)
            //we will run into issues if the animator transitions to another animation and the event never triggers...


        protected virtual void Awake()
        {
            combatant = GetComponent<Combatant>();

            combatManager = new(null, combatant, GetComponent<AnimationControl>(), timeToLeaveCombat);
            combatManager.Configure();

            MovementController = GetComponent<IMovementController>();
            InputSource = GetComponent<IInputSource>();

            tickTimer = GetComponent<TickTimer>();
            tickTimer.RandomizeStartValue = true;

            InitializeAbilityBars();
        }

        protected virtual void OnEnable()
        {
            tickTimer.NewTickEvent += combatManager.OnNewTick;

            OnChannelEnded += () => combatManager.animationControl.animator.SetTrigger("ceaseAttack");

            combatManager.StateGraph.inCombat.OnEntry += OnCombatEntry;
            combatManager.StateGraph.inCombat.OnExit += OnCombatExit;
            combatManager.StateGraph.dead.OnEntry += Death;
            combatManager.StateGraph.dead.OnExit += Revival;
            combatManager.OnWeaponTick += OnWeaponTick;
        }

        protected virtual void Start()
        {
            combatant.OnTargetingFailed += OnTargetingFailed;
            combatant.OnWeaponEquip += OnWeaponEquip;
            combatant.Health.OnStunned += async (duration, freezeAnimation) => 
                await GetStunned(duration, freezeAnimation, GlobalGameTools.Instance.TokenSource);
            combatant.Health.HealthChanged += OnHealthChanged;

            combatant.EquipWeaponSO();
        }

        protected virtual void Update()
        {
            if(FireButtonIsDown)
            {
                RunAutoAbilityCycle(false);
            }
        }

        protected virtual void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }


        //SETUP

        private void InitializeAbilityBars()
        {
            unarmedAbilityBar = useDefaultAbilityBars ?
                new(this, CurrentAbilityBar.GetDefaultAbilityBarItems(CombatStyle.Unarmed))
                : new(this, unarmedAbilityBarItems?.Select(x => x.CreateAbilityBarItem()).ToList());
            mageAbilityBar = useDefaultAbilityBars ?
                new(this, CurrentAbilityBar.GetDefaultAbilityBarItems(CombatStyle.Mage))
                : new(this, mageAbilityBarItems?.Select(x => x.CreateAbilityBarItem()).ToList());
            meleeAbilityBar = useDefaultAbilityBars ?
                new(this, CurrentAbilityBar.GetDefaultAbilityBarItems(CombatStyle.Melee))
                : new(this, meleeAbilityBarItems?.Select(x => x.CreateAbilityBarItem()).ToList());
            rangedAbilityBar = useDefaultAbilityBars ?
                new(this, CurrentAbilityBar.GetDefaultAbilityBarItems(CombatStyle.Ranged))
                : new(this, rangedAbilityBarItems?.Select(x => x.CreateAbilityBarItem()).ToList());

            GetAbilityBar = new()
            {
                [CombatStyle.Unarmed] = unarmedAbilityBar,
                [CombatStyle.Melee] = meleeAbilityBar,
                [CombatStyle.Mage] = mageAbilityBar,
                [CombatStyle.Ranged] = rangedAbilityBar
            };
        }


        //ABILITY INPUT HANDLING

        protected virtual void HandleAbilityInput(int index)
        {
            ExecuteAbility(currentAbilityBar.GetAbility(index));
        }

        public virtual void RunAutoAbilityCycle(bool runOffGCD)
        {
            if (!postCancellationLock && !ChannelingAbility && ((queuedAbility == null && !GlobalCooldown) || runOffGCD))
            {
                ExecuteAbility(currentAbilityBar?.GetAutoCastAbility());
            }
        }

        protected virtual bool CanExecute(AttackAbility ability)
        {
            return !currentAbilityBar.OnCooldown(ability) 
                && (!ability.ObeyGCD || !GlobalCooldown)
                && (combatant.Weapon?.CombatStyle == ability.CombatStyle || ability.CombatStyle == CombatStyle.Any);
        }

        protected virtual void ExecuteAbility(AttackAbility ability)
        {
            if (ability == null) return;

            if (CanExecute(ability))
            {
                CancelAbilityInProgress(false);
                ability?.Execute(this);
            }
            else if(!currentAbilityBar.OnCooldown(ability))
            {
                queuedAbility = ability;
            }
            else
            {
                AttemptedToExecuteAbilityOnCooldown();
            }
        }

        protected virtual void CancelAbilityInProgress(bool delayedReleaseOfChannel = false)
        {
            EndChannel(delayedReleaseOfChannel);
            combatant.CombatantReset();
        }

        protected virtual void FireButtonDown()
        {
            BaseOnFireButtonDown();
            OnFireButtonDown?.Invoke();//because we can't invoke these events from derived classes...
        }

        protected virtual void FireButtonUp()
        {
            BaseOnFireButtonUp();
            OnFireButtonUp?.Invoke();
        }

        protected virtual void BaseOnFireButtonDown()
        {
            queuedAbility = null;
            FireButtonIsDown = true;
            FaceAimPosition();
            combatant.Stamina.autoReplenish = false;
            RunAutoAbilityCycle(true);
        }

        protected virtual void BaseOnFireButtonUp()
        {
            FireButtonIsDown = false;
            combatant.Stamina.autoReplenish = true;
        }

        protected virtual void AttemptedToExecuteAbilityOnCooldown() { }

        protected virtual void DisableInput()
        {
            CancelAbilityInProgress(false);
            InputSource?.DisableInput();
        }

        protected virtual void EnableInput()
        {
            InputSource?.EnableInput();
        }


        //ABILITY INTERFACING

        public virtual void StartChannel()
        {
            ChannelingAbility = true;
            OnChannelStarted?.Invoke();
        }

        public virtual void EndChannel(bool delayReleaseUntilFireButtonUp = false)
        {
            if (!postCancellationLock)
            {
                if (FireButtonIsDown && delayReleaseUntilFireButtonUp)
                {
                    postCancellationLock = true;
                    OnFireButtonUp += DelayedRelease;

                    void DelayedRelease()
                    {
                        ChannelingAbility = false;
                        postCancellationLock = false;
                        OnFireButtonUp -= DelayedRelease;
                    }
                }
                else
                {
                    ChannelingAbility = false;
                }
            }

            EndPowerUp();
            queuedAbility = null;
            StoredAction = null;
            StopAimingAction();
            OnChannelEnded?.Invoke();
        }

        public virtual void StartPowerUp(AttackAbility ability)
        {
            PoweringUp = true;
            OnPowerUpStarted?.Invoke(); 
            if (ability.UseActiveAimingWhilePoweringUp)
            {
                StartAimingAction(ActiveAiming);
            }
        }

        public virtual void EndPowerUp()
        {
            PoweringUp = false;
            OnPowerUpEnded?.Invoke();
        }

        public virtual void StartCooldown(AttackAbility ability)
        {
            OnCooldownStarted?.Invoke(ability);
        }

        public virtual void MaximumPowerAchieved()
        {
            OnMaximumPowerAchieved?.Invoke();
        }

        public virtual void OnAbilityExecute(AttackAbility ability)
        {
            queuedAbility = null;
            GlobalCooldown = true;
            StartCooldown(ability);
            PlayAnimation(ability.AnimationState, ability.CombatStyle);
            if (ability.HoldAimOnRelease)
            {
                HoldAim(750);
            }
        }

        public virtual void OnTargetingFailed() { }

        public virtual void OnInsufficientStamina() { }

        public virtual void OnInsufficientWrath() { }

        public void StoreAction(Action action)//these functions would be better in the animation control class
        {
            StoredAction = action;
            Debug.Log("stored action");
            //gets cleared in OnChannel (in particular if you cast an ability, exit combat, or die)
        }

        public void ExecuteStoredAction()
        {
            Debug.Log("executing stored action");
            Debug.Log($"is the stored action null? {StoredAction == null}");
            StoredAction?.Invoke();
            StoredAction = null;
        }

        //STATE TRANSITIONS

        public virtual void OnWeaponEquip()
        {
            EndChannel();
            combatant.ReturnQueuedProjectileToPool();

            GetAbilityBar.TryGetValue(combatant.Weapon.CombatStyle, out var abilityBar);
            currentAbilityBar = abilityBar;
            if (currentAbilityBar != null && !currentAbilityBar.Configured)
            {
                currentAbilityBar.Configure();
            }

            AbilityBarResetEvent?.Invoke();
        }

        public virtual void OnCombatEntry()
        {
            CombatEntered?.Invoke();
        }

        public virtual void OnCombatExit()
        {
            EndChannel();//kills queued ability
            GlobalCooldown = false;
            CombatExited?.Invoke();
        }

        protected virtual void OnWeaponTick()
        {
            GlobalCooldown = false;
            ExecuteAbility(queuedAbility);
        }

        public virtual void PlayAnimation(string stateName, CombatStyle combatStyle)
        {
            combatManager.animationControl.PlayAnimationState(stateName, combatStyle.ToString(), 0);
        }

        public virtual void PlayPowerUpAnimation(string stateName, CombatStyle combatStyle)
        {
            PlayAnimation(stateName + " PowerUp", combatStyle);
        }

        public virtual void PlayChannelAnimation(string stateName, CombatStyle combatStyle)
        {
            PlayAnimation(stateName + " Channel", combatStyle);
        }


        //AIMING FUNCTIONS

        public virtual Vector2 GetAimPosition()
        {
            return default;
        }

        public virtual void FaceAimPosition()
        {
            MovementController?.FaceTarget(GetAimPosition());
        }

        protected void StartAimingAction(Action aimingAction)
        {
            StopAimingAction();
            CurrentAimingAction = aimingAction;
            OnLateUpdate += CurrentAimingAction;
            aimingActionInUse = true;
        }

        protected void StopAimingAction()
        {
            OnLateUpdate -= CurrentAimingAction;
            CurrentAimingAction = null;
            aimingActionInUse = false;
        }

        public virtual float ComputeAimAngle()//rotate chest so that mainhand forearm points toward aim position
        {
            bool moving = MovementController.Rigidbody.linearVelocity.magnitude > Mathf.Epsilon;
            if (FireButtonIsDown && !moving)
            {
                FaceAimPosition();
            }

            Vector2 aimPos = GetAimPosition();
            bool facingRight = MovementController.CurrentOrientation == HorizontalOrientation.right;

            if (moving)//if moving reflect the point to be in front of you
            {
                if ((facingRight && aimPos.x < transform.position.x) || (!facingRight && aimPos.x > transform.position.x))
                {
                    aimPos -= 2 * (aimPos.x - transform.position.x) * Vector2.right;
                }
            }

            Vector2 aimVector = aimPos - (Vector2)combatant.MainhandElbow.transform.position;
            Vector2 forearmVector = combatant.EquipSlots[ItemSlot.EquipmentSlots.Mainhand].transform.position - combatant.MainhandElbow.transform.position;
            float deltaAngle = Vector2.SignedAngle(forearmVector, aimVector);

            float angle = combatant.ChestBone.eulerAngles.z + deltaAngle;
            angle -= (float) Math.Floor(angle / 360) * 360;//take angle % 360

            if(facingRight)
            {
                return Mathf.Clamp(angle, 60, 150);
            }
            else
            {
                return Mathf.Clamp(angle, 210, 300);
            }
        }

        protected void RotateToAngle(float angle)
        {
            combatant.ChestBone.eulerAngles = angle * Vector3.forward;
        }

        protected void ActiveAiming()
        {
            RotateToAngle(ComputeAimAngle());
        }

        protected void WhileHoldingAimAngle(float angle)
        {
            bool facingRight = MovementController.CurrentOrientation == HorizontalOrientation.right;
            if ((facingRight && angle > 180) || (!facingRight && angle <= 180))
            {
                angle = 360 - angle;
            }
            RotateToAngle(angle);
        }

        public async void HoldAim(int duration)
        {
            if (duration == 0) return;
            float angle = ComputeAimAngle();
            Action localAimAction = () => WhileHoldingAimAngle(angle);
            StartAimingAction(localAimAction);
            await Task.Delay(duration);

            if (CurrentAimingAction == localAimAction)
            {
                StopAimingAction();
            }
        }


        //HANDLE INCOMING DAMAGE

        public virtual void OnHealthChanged(float damage, IDamageDealer damageDealer)
        {
            if (combatManager.StateMachine.CurrentState is not InCombat && damage > 0)
            {
                combatant.Attack();
            }
            float effectiveDamage = combatant.HandleHealthChange(damage, damageDealer);
            combatManager.HandleHealthChange(effectiveDamage);
            HealthChangeEffected?.Invoke(effectiveDamage);
        }

        protected async Task GetStunned(float stunDuration, bool freezeAnimation, 
            CancellationTokenSource tokenSource)
        {
            if (combatant.Health.IsDead || immuneToStuns) return;

            var stunData = (stunDuration, freezeAnimation);
            activeStuns.Add(stunData);
            DisableInput();
            if (freezeAnimation)
            {
                combatManager.Freeze();
            }

            TaskCompletionSource<object> tcs = new();
            //we don't need to register cancellation of tcs to the tokenSource,
            //b/c we're awaiting Task.WhenAny(...) where the other task Task.Delay will get cancelled.
            //A cancelled task will cause Task.WhenAny() to complete, but it doesn't rethrow the
            //TaskCancelledException (so don't get worried if you don't see any exceptions after cancelling)
            //NOTE ALSO: we could maybe use a linked CTS instead of TCS, but then we have to catch the cancellation and
            //figure out within the catch clause whether we got cancelled because we died or because the game ended,
            //so it's just messier


            void CompleteEarly()
            {
                tcs.TrySetResult(null);
            }

            try
            {
                combatManager.StateGraph.dead.OnEntry += CompleteEarly;
                Task result = await Task.WhenAny(MiscTools.DelayGameTime(stunDuration, tokenSource.Token), tcs.Task);
                if (tokenSource.IsCancellationRequested) return;

                activeStuns.Remove(stunData);

                if (freezeAnimation && activeStuns.Where(x => x.Item2 = true).Count() == 0)
                {
                    combatManager.Unfreeze();
                }
                if (activeStuns.Count == 0 && !combatant.Health.IsDead)
                {
                    InputSource.EnableInput();
                }
            }
            finally
            {
                combatManager.StateGraph.dead.OnEntry -= CompleteEarly;
            }
        }


        //PAUSE, DEATH, AND DESTROY HANDLERS

        public void Pause()
        {
            DisableInput();
        }

        public void Unpause()
        {
            EnableInput();
        }

        protected virtual void Death()
        {
            DisableInput();
            combatManager.animationControl.Freeze(false);//UNFREEZE, in case animation was frozen due to a stun
            combatant.OnDeath();
            MovementController?.OnDeath();
            OnDeath?.Invoke();
        }

        protected virtual void Revival()
        {
            combatant.EquipSlots[ItemSlot.EquipmentSlots.Mainhand].gameObject.SetActive(true);
            MovementController?.OnRevival();
            EnableInput();
            OnRevive?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            AbilityBarResetEvent = null;
            OnCooldownStarted = null;
            OnFireButtonUp = null;
            OnFireButtonDown = null;
            OnChannelStarted = null;
            OnChannelEnded = null;
            OnPowerUpStarted = null;
            OnPowerUpEnded = null;
            OnMaximumPowerAchieved = null;
            StopAimingAction();
            OnLateUpdate = null;
            OnDeath = null;
            OnRevive = null;
            HealthChangeEffected = null;
            StoredAction = null;
        }
    }
}