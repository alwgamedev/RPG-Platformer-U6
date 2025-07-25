﻿using RPGPlatformer.Core;
using RPGPlatformer.Movement;
using System.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;
using RPGPlatformer.AIControl;

namespace RPGPlatformer.Combat
{
    [RequireComponent(typeof(TickTimer))]
    [RequireComponent(typeof(AnimationControl))]
    public class GenericCombatController<T0, T1, T2, T3, T4> : StateDrivenController<T0, T1, T2, T3>,
        ICombatController, IAbilityBarOwner,/* IPausable,*/ IInputDependent
        where T0 : CombatStateManager<T1, T2, T3, T4>
        where T1 : CombatStateGraph
        where T2 : CombatStateMachine<T1>
        where T3 : Combatant
        where T4 : AnimationControl
    {
        [SerializeField] protected float timeToLeaveCombat = 200;
        [SerializeField] protected SerializableCharacterAbilityBarData abilityBarData = new();
        [SerializeField] protected bool useDefaultAbilityBars;
        [SerializeField] protected bool immuneToStuns = false;

        //protected Combatant combatant;
        //protected CombatStateManager combatManager;
        protected CombatantAbilityBarManager abilityBarManager;

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

        protected int activeStuns;
        protected int activeStunsThatFreezeAnimation;

        public bool IsInCombat => stateManager.StateMachine.CurrentState is InCombat;
        public AbilityBar CurrentAbilityBar => abilityBarManager?.CurrentAbilityBar;
        public TickTimer TickTimer => tickTimer;
        public bool GlobalCooldown { get; protected set; }
        public bool ChannelingAbility { get; protected set; }
        public bool PoweringUp { get; protected set; }
        public bool FireButtonIsDown { get; protected set; }
        public bool HasStoredAction => StoredAction != null;
        public ICombatant Combatant => stateDriver;
        public ICombatantMovementController MovementController { get; protected set; }
        public virtual IInputSource InputSource { get; protected set; }

        public event Action OnDisabled;
        public event Action CombatManagerConfigured;
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
        //^parameter represents damage (so positive is damage taken, and negative is health gained)


        protected override void Awake()
        {
            base.Awake();

            MovementController = GetComponent<ICombatantMovementController>();
            InitializeInputSource();

            OnDisabled += () => EndChannel();
        }

        protected override void Start()
        {
            base.Start();

            InitializeAbilityBarManager();

            tickTimer = GetComponent<TickTimer>();
            tickTimer.AddRandomizedOffset();
            tickTimer.NewTick += stateManager.OnNewTick;

            stateDriver.OnTargetingFailed += OnTargetingFailed;
            stateDriver.OnWeaponEquip += OnWeaponEquip;
            stateDriver.Health.OnStunned += async (duration, freezeAnimation) =>
                await GetStunned(duration, freezeAnimation, GlobalGameTools.Instance.TokenSource);
            stateDriver.Health.HealthChangeTrigger += OnHealthChanged;

            stateDriver.OnInventoryOverflow += OnInventoryOverflow;
            InitializeCombatantEquipment();
            InitializeInventoryItems();

            stateDriver.OnStart();
        }

        protected virtual void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }


        //SET-UP

        public void InitializeInputSource()
        {
            InputSource = GetComponent<IInputSource>();
        }

        protected override void InitializeStateManager()
        {
            stateManager = (T0)Activator.CreateInstance(typeof(T0), null, stateDriver, 
                GetComponentInChildren<AnimationControl>(), timeToLeaveCombat);
        }

        protected override void ConfigureStateManager()
        {
            base.ConfigureStateManager();

            stateManager.StateGraph.inCombat.OnEntry += OnCombatEntry;
            stateManager.StateGraph.inCombat.OnExit += OnCombatExit;
            stateManager.StateGraph.dead.OnEntry += Death;
            stateManager.StateGraph.dead.OnExit += Revival;
            stateManager.OnWeaponTick += OnWeaponTick;

            CombatManagerConfigured?.Invoke();
        }

        protected virtual void InitializeAbilityBarManager()
        {
            abilityBarManager = new CombatantAbilityBarManager(this);
            InitializeAbilityBarData();
            InitializeAbilityBars();
            UpdateEquippedAbilityBar();
        }

        //this gives you the option to get data from somewhere different then the SF
        protected virtual void InitializeAbilityBarData()
        {
            if (useDefaultAbilityBars)
            {
                abilityBarData = SerializableCharacterAbilityBarData.DefaultAbilityBarData();
            }
        }

        protected virtual void InitializeAbilityBars()
        {
            abilityBarManager.UpdateAbilityBars(abilityBarData);
        }

        protected virtual void InitializeInventoryItems() { }

        protected virtual void InitializeCombatantEquipment()
        {
            stateDriver.EquipDefaultItems();
        }


        //ABILITY BARS

        protected virtual void UpdateAbilityBars(SerializableCharacterAbilityBarData data)
        {
            abilityBarManager.UpdateAbilityBars(data);
            UpdateEquippedAbilityBar();
        }

        protected virtual void UpdateEquippedAbilityBar()
        {
            abilityBarManager.EquipAbilityBar(stateDriver.CurrentCombatStyle);
            AbilityBarResetEvent?.Invoke();
        }


        //ABILITY INPUT HANDLING

        protected virtual void HandleAbilityInput(int index)
        {
            TryExecuteAbility(CurrentAbilityBar.GetAbility(index));
        }

        //returns whether an ability was executed
        //(but that ability may still fail to fully execute if you have insufficient wrath or stamina)
        public virtual bool RunAutoAbilityCycle(bool runOffGCD)
        {
            if (!postCancellationLock && !ChannelingAbility
                && ((queuedAbility == null && !GlobalCooldown) || runOffGCD))
            {
                var a = CurrentAbilityBar?.GetAutoCastAbility();
                if (a != null)
                {
                    return TryExecuteAbility(a);
                }
                else
                {
                    OnNoAutoCastAbility();
                }
            }

            return false;
        }

        protected virtual void OnNoAutoCastAbility() { }

        protected enum CanExecuteResult
        {
            pass, onCooldown, gcdBlock, wrongCombatStyle, noAbility, insufficientStats
        }

        protected virtual CanExecuteResult CanExecute(AttackAbility ability)
        {
            //note the order is important! we will only return gcdBlock if it is off cooldown and passes
            //all other tests; then it can be queued
            if (ability == null)
            {
                return CanExecuteResult.noAbility;
            }
            if (stateDriver.EquippedWeapon?.CombatStyle != ability.CombatStyle && ability.CombatStyle != CombatStyle.Any)
            {
                return CanExecuteResult.wrongCombatStyle;
            }
            if (CurrentAbilityBar.IsOnCooldown(ability))
            {
                return CanExecuteResult.onCooldown;
            }
            if (!ability.CanBeExecuted(this))
            {
                return CanExecuteResult.insufficientStats;
            }
            if (ability.ObeyGCD && GlobalCooldown)
            {
                return CanExecuteResult.gcdBlock;
            }

            return CanExecuteResult.pass;
        }

        //returns whether ability was execute
        //(but that ability may still fail to fully execute if you have insufficient wrath or stamina)
        protected virtual bool TryExecuteAbility(AttackAbility ability)
        {
            var r = CanExecute(ability);

            switch(r)
            {
                case CanExecuteResult.noAbility:
                    return false;
                case CanExecuteResult.wrongCombatStyle:
                    return false;
                case CanExecuteResult.onCooldown:
                    AttemptedToExecuteAbilityOnCooldown();
                    return false;
                case CanExecuteResult.insufficientStats:
                    return false;
                case CanExecuteResult.gcdBlock:
                    queuedAbility = ability;
                    return false;
                case CanExecuteResult.pass:
                    ExecuteAbility(ability);
                    return true;
            }

            return false;
        }

        protected virtual void ExecuteAbility(AttackAbility ability)
        {
            if (ChannelingAbility)
            {
                CancelAbilityInProgress();
            }
            ability.Execute(this);
        }

        protected virtual void CancelAbilityInProgress(bool delayedReleaseOfChannel = false)
        {
            EndChannel(delayedReleaseOfChannel);
            stateManager.CeaseAttack();
            stateDriver.CombatantReset();
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
            if (MovementController != null && !MovementController.Moving)
            {
                FaceAimPosition();
            }
            stateDriver.Stamina.autoReplenish = false;
            RunAutoAbilityCycle(true);
        }

        protected virtual void BaseOnFireButtonUp()
        {
            FireButtonIsDown = false;
            stateDriver.Stamina.autoReplenish = true;
        }

        protected virtual void AttemptedToExecuteAbilityOnCooldown() { }

        public virtual void OnInputEnabled() { }

        public virtual void OnInputDisabled()
        {
            FireButtonUp();
            //^so that auto ability cycle stops running if you have fire button down
            CancelAbilityInProgress();
        }


        //ABILITY INTERFACING

        public virtual void StartChannel()
        {
            ChannelingAbility = true;
            OnChannelStarted?.Invoke();
        }

        //so that we can call in anim event
        public void EndChannel()
        {
            EndChannel(false);
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

        public void StoreAction(Action action, bool channelWhileStored = true, bool endChannelOnExecute = true)
        //these functions would be better in the animation control class?
        {
            StoredAction = action;
            if (channelWhileStored)
            {
                if (!ChannelingAbility)
                {
                    StartChannel();//so that auto-cast cycle will not interrupt the animation
                }
                if (endChannelOnExecute)
                {
                    StoredAction += () => EndChannel();
                }
                //if don't end channel on execute, then you need to make sure you end channel in the animation
                //(making sure the anim event won't get cut off by a transition)
            }
        }

        public void ExecuteStoredAction()
        {
            StoredAction?.Invoke();
            StoredAction = null;
        }

        //HANDLE COMBATANT STATE

        public virtual void OnWeaponEquip()
        {
            //EndChannel();
            CancelAbilityInProgress();//new
            stateDriver.ReturnQueuedProjectileToPool();
            UpdateEquippedAbilityBar();
            stateManager.OnWeaponEquip();
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
            TryExecuteAbility(queuedAbility);
        }

        public virtual void PlayAnimation(string stateName, CombatStyle combatStyle)
        {
            stateManager.animationControl.PlayAnimationState(stateName, combatStyle.ToString(), 0);
        }

        public virtual void PlayPowerUpAnimation(string stateName, CombatStyle combatStyle)
        {
            PlayAnimation(stateName + " PowerUp", combatStyle);
        }

        public virtual void PlayChannelAnimation(string stateName, CombatStyle combatStyle)
        {
            PlayAnimation(stateName + " Channel", combatStyle);
        }

        protected virtual void OnInventoryOverflow() { }


        //AIMING FUNCTIONS

        public virtual Vector2 GetAimPosition()
        {
            if (MovementController != null)
            {
                return transform.position + (int)MovementController.CurrentOrientation * transform.right;
            }

            return transform.position;
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

        public virtual float ComputeAimAngleChange()//rotate chest so that mainhand forearm points toward aim position
        {
            //var moving = MovementController.RelativeVelocity.magnitude > Mathf.Epsilon;
            var moving = MovementController.Moving;
            var aimPos = GetAimPosition();
            var facingRight = MovementController.CurrentOrientation == HorizontalOrientation.right;

            if (FireButtonIsDown && !moving)
            {
                FaceAimPosition();
            }

            if (moving)//if moving reflect the point to be in front of you
            {
                if ((facingRight && aimPos.x < transform.position.x)
                    || (!facingRight && aimPos.x > transform.position.x))
                {
                    aimPos -= 2 * (aimPos.x - transform.position.x) * Vector2.right;
                }
            }

            var mainHand = stateDriver.EquipSlots[EquipmentSlot.Mainhand].transform;
            var aimVector = aimPos - (Vector2)mainHand.position;
            var forearmVector = mainHand.position
                - stateDriver.MainhandElbow.transform.position;

            return Vector2.SignedAngle(forearmVector, aimVector);
            //NOTE: animations should set mainhand slot angle to 0 (in case something like
            //the wall cling animation is overriding mainhand slot rotation)
        }

        protected void RotateChest(float angle)
        {
            stateDriver.ChestBone.Rotate(Vector3.forward, angle);
        }

        protected void ActiveAiming()
        {
            RotateChest(ComputeAimAngleChange());
        }

        protected void WhileHoldingAimAngle(float angle)
        {
            bool facingRight = MovementController.CurrentOrientation == HorizontalOrientation.right;
            if ((facingRight && angle > 180) || (!facingRight && angle <= 180))
            {
                angle = 360 - angle;
            }
            RotateChest(angle);
        }

        public async void HoldAim(int duration)
        {
            if (duration == 0) return;
            var angle = ComputeAimAngleChange();
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
            if (damage > 0)
            {
                stateDriver.TriggerCombat();
            }
            float effectiveDamage = stateDriver.HandleHealthChange(damage, damageDealer);
            if (!ChannelingAbility)//so that take dmg animation doesn't mess up any drawn out combat animations
            {
                stateManager.AnimateHealthChange(effectiveDamage);
            }
            HealthChangeEffected?.Invoke(effectiveDamage);
        }

        protected async Task GetStunned(float stunDuration, bool freezeAnimation,
            CancellationTokenSource tokenSource)
        {
            if (stateDriver.Health.IsDead || immuneToStuns) return;

            activeStuns++;
            InputSource?.DisableInput();
            if (freezeAnimation)
            {
                activeStunsThatFreezeAnimation++;
                stateManager.Freeze();
            }

            TaskCompletionSource<object> tcs = new();
            //using a tcs instead of linked cts or registration, because we need to do some additional stuff
            //even after cancellation (and when linked cts has been cancelled, it takes a bit of extra
            //effort to figure out what was the source of cancellation and whether we should still do
            //the "extra stuff")

            void CompleteEarly()
            {
                tcs.TrySetResult(null);
            }

            try
            {
                OnStunned(freezeAnimation);
                stateManager.StateGraph.dead.OnEntry += CompleteEarly;
                OnDisabled += CompleteEarly;
                await Task.WhenAny(MiscTools.DelayGameTime(stunDuration, tokenSource.Token), tcs.Task);
                if (tokenSource.IsCancellationRequested) return;

                activeStuns--;

                if (freezeAnimation /*&& activeStuns.Where(x => x.Item2 = true).Count() == 0*/)
                {
                    activeStunsThatFreezeAnimation--;
                    if (activeStunsThatFreezeAnimation == 0 && !stateDriver.Health.IsDead)
                    {
                        //note if died while stunned, state machine will have already unfrozen
                        stateManager.Unfreeze();
                    }
                }
                if (activeStuns == 0 && !stateDriver.Health.IsDead)
                {
                    InputSource?.EnableInput();
                }
            }
            finally
            {
                stateManager.StateGraph.dead.OnEntry -= CompleteEarly;
                OnDisabled -= CompleteEarly;
            }
        }

        protected virtual void OnStunned(bool frozen) { }


        //PAUSE, DEATH, AND DESTROY HANDLERS

        protected virtual async void Death()
        {
            InputSource?.DisableInput();
            stateDriver.OnDeath();
            MovementController?.OnDeath();
            OnDeath?.Invoke();

            await stateDriver.FinalizeDeath(GlobalGameTools.Instance.TokenSource.Token);
        }

        protected virtual void Revival()
        {
            //stateDriver.EquipSlots[EquipmentSlot.Mainhand].gameObject.SetActive(true);
            stateDriver.OnRevival();
            MovementController?.OnRevival();
            InputSource?.EnableInput();
            OnRevive?.Invoke();
        }

        protected virtual void OnDisable()
        {
            OnDisabled?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            OnDisabled = null;
            CombatManagerConfigured = null;
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