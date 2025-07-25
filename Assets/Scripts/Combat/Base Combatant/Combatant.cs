﻿using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Effects;
using RPGPlatformer.Inventory;
using RPGPlatformer.Loot;
using RPGPlatformer.Skills;
using System.Threading.Tasks;
using System.Threading;
using RPGPlatformer.SceneManagement;

namespace RPGPlatformer.Combat
{
    using static Health;

    [RequireComponent(typeof(CharacterProgressionManager))]
    [RequireComponent(typeof(InventoryManager))]
    [RequireComponent(typeof(DropSpawner))]
    [RequireComponent(typeof(Health))]
    public class Combatant : StateDriver, ICombatant, IInventoryOwner, ILooter, ILootDropper, IOutOfBoundsHandler
    {
        [SerializeField] protected string displayName;
        [SerializeField] protected LayerMask targetLayerMask;
        [SerializeField] protected ItemSlot headSlot;
        [SerializeField] protected ItemSlot torsoSlot;
        [SerializeField] protected ItemSlot legsSlot;
        [SerializeField] protected ItemSlot mainhandSlot;
        [SerializeField] protected ItemSlot offhandSlot;
        [SerializeField] protected Transform mainhandElbow;
        [SerializeField] protected Transform chestBone;
        [SerializeField] protected WeaponSO unarmedWeaponSO;
        [SerializeField] protected ReplenishableStat stamina = new();
        [SerializeField] protected ReplenishableStat wrath = new();
        [SerializeField] protected bool useAutoCalculatedHealthPoints;
        [SerializeField] protected bool dropLootOnFinalizeDeath = true;
        [SerializeField] protected bool destroyOnFinalizeDeath = true;
        [SerializeField] protected bool delayBeforeFinalizeDeath = true;
        [SerializeField] protected bool finalizeDeathViaTrigger = false;
        [SerializeField] protected float timeToDelayBeforeFinalizeDeath = 1.5f;
        [SerializeField] protected float minimumCombatDistanceMultiplier = 0.5f;

        protected CharacterProgressionManager progressionManager;
        protected CombatBonusesManager combatBonusesManager;
        protected InventoryManager inventory;
        protected Dictionary<EquipmentSlot, ItemSlot> equipSlots = new();
        protected DropSpawner dropSpawner;
        protected Weapon equippedWeapon;
        protected Weapon unarmedWeapon;
        protected Health health;
        protected Action FinalizeDeathTrigger;
        //protected int _targetLayer;
        //protected Collider2D[] targetingStorage;

        public string DisplayName => $"<b>{displayName}</b>";
        public int CombatLevel => progressionManager.CombatLevel;
        public virtual bool IsPlayer => false;
        public CharacterProgressionManager ProgressionManager => progressionManager;
        public InventoryManager Inventory => inventory;
        public Dictionary<EquipmentSlot, ItemSlot> EquipSlots => equipSlots;
        public Transform MainhandElbow => mainhandElbow;
        public Transform ChestBone => chestBone;
        public Transform ProjectileAnchor
        {
            get
            {
                if (mainhandSlot.EquippedItemGO && mainhandSlot.EquippedItemGO.ProjectileAnchor)
                {
                    return mainhandSlot.EquippedItemGO.ProjectileAnchor;
                }

                return mainhandSlot.transform;
            }
        }
        public float AttackRange { get; protected set; }
        public float IdealMinimumCombatDistance { get; protected set; }
        public IWeapon EquippedWeapon => equippedWeapon;
        public IWeapon UnarmedWeapon => unarmedWeapon;
        public CombatStyle CurrentCombatStyle => equippedWeapon?.CombatStyle ?? CombatStyle.Unarmed;
        public IProjectile QueuedProjectile { get; set; }
        public IHealth Health => health;
        public ReplenishableStat Stamina => stamina;
        public ReplenishableStat Wrath => wrath;
        public Vector2 DropPosition => health.HitEffectTransform.position;
        public bool DestroyOnFinalizeDeath { get => destroyOnFinalizeDeath; set => destroyOnFinalizeDeath = value; }

        public event Action OnTargetingFailed;
        public event Action OnWeaponEquip;
        public event Action OnInventoryOverflow;
        public event Action DeathFinalized;
        public event Action AfterDeathFinalized;

        protected virtual void Awake()
        {
            health = GetComponent<Health>();
            progressionManager = GetComponent<CharacterProgressionManager>();
            inventory = GetComponent<InventoryManager>();
            dropSpawner = GetComponent<DropSpawner>();

            progressionManager.StateRestored += OnProgressionManagerStateRestored;
            progressionManager.LevelUp += OnLevelUp;
            
            combatBonusesManager = new(this);
            //_targetLayer = LayerMask.GetMask(targetLayer);

            equipSlots = new();
            
            if (headSlot != null)
            {
                equipSlots[EquipmentSlot.Head] = headSlot;
            }
            if (torsoSlot != null)
            {
                equipSlots[EquipmentSlot.Torso] = torsoSlot;
            }
            if (legsSlot != null)
            {
                equipSlots[EquipmentSlot.Legs] = legsSlot;
            }
            if (mainhandSlot != null)
            {
                equipSlots[EquipmentSlot.Mainhand] = mainhandSlot;
            }
            if (offhandSlot != null)
            {
                equipSlots[EquipmentSlot.Offhand] = offhandSlot;
            }

            OnWeaponEquip += UpdateAttackRange;

            InitializeUnarmedWeapon();
        }

        //private void Start()
        //{
        //    //ConfigureReplenishableStats();
        //    InitializeMinCombatDistance();
        //}

        private void Update()
        {
            //auto-replenish stats if enabled
            stamina.Update();
            wrath.Update();
        }

        public virtual void OnStart()
        {
            ConfigureReplenishableStats();
            InitializeMinCombatDistance();
        }

        public virtual void InitializeMinCombatDistance()
        {
            IdealMinimumCombatDistance = unarmedWeapon.WeaponStats.AttackRange * minimumCombatDistanceMultiplier;
        }


        //STATS

        public int GetLevel(CharacterSkill skill)
        {
            return progressionManager.GetLevel(skill);
        }

        public float AdditiveDamageBonus()
        {
            return combatBonusesManager.AdditiveDamageBonus();
        }

        public float DamageTakenMultiplier()
        {
            return combatBonusesManager.DamageTakenMultiplier();
        }

        //NOTE: note sure whether this should be done through combatant or combatcontroller
        //(bc some debuffs may affect movement, in which case we want to go through the controller
        //who has reference to movement controller)
        //-- for now "SlowMovement()" is with the controller, so when we get around to doing
        //the debuff system we may want to unite things
        public void BeginDebuff(/*debuff data or IDebuff(Data)*/) { }

        //temporary until we implement the debuff system
        public void SetInvincible(bool val)
        {
            combatBonusesManager.invincible = val;
        }



        //STATS

        protected virtual void ConfigureReplenishableStats()
        {
            if(useAutoCalculatedHealthPoints)
            {
                SetAutoCalculatedHealthPoints();
            }

            stamina.autoReplenish = true;
            //health and wrath auto-replenish will change when you enter and exit combat.
            //this is set up by the combat manager

            stamina.Depleted += OnStaminaDepleted;
            wrath.Depleted += OnWrathDepleted;

            Health.Stat.TakeDefaultValue();
            stamina.TakeDefaultValue();
            wrath.TakeDefaultValue();
        }

        protected virtual void SetAutoCalculatedHealthPoints(bool maintainFraction = false)
        {
            Health.Stat.SetMaxAndDefaultValue(progressionManager.AutoCalculatedHealthPoints, maintainFraction);
        }

        protected virtual void OnLevelUp(ICharacterSkill skill, int level)
        {
            if (useAutoCalculatedHealthPoints && skill == CharacterSkillBook.Fitness)
            {
                SetAutoCalculatedHealthPoints();
            }
        }

        protected virtual void OnStaminaDepleted() { }

        protected virtual void OnWrathDepleted() { }

        protected virtual void UpdateAttackRange()
        {
            AttackRange = equippedWeapon?.WeaponStats.AttackRange ?? 0;
        }

        protected virtual void OnProgressionManagerStateRestored()
        {
            SetAutoCalculatedHealthPoints(true);
        }


        //BASIC FUNCTIONS

        //note this is not subscribed to any events. cc calls it directly
        public virtual float HandleHealthChange(float damage, IDamageDealer damageDealer)
        {
            if (damage > 0)
            {
                damage *= DamageTakenMultiplier();
            }
            health.GainHealth(-damage, true);
            if (health.Stat.CurrentValue <= health.Stat.MinValue)
            {
                Die(damageDealer);
            }
            return damage;
        }

        public virtual void OnDeath()
        {
            wrath.CurrentValue = 0;
            if (mainhandSlot)
            {
                mainhandSlot.gameObject.SetActive(false);
            }
            if (offhandSlot)
            {
                offhandSlot.gameObject.SetActive(false);
            }
        }

        public virtual void OnRevival()
        {
            if (mainhandSlot)
            {
                mainhandSlot.gameObject.SetActive(true);
            }
            if (offhandSlot)
            {
                offhandSlot.gameObject.SetActive(false);
            }
        }

        //in case you want to finalize death via animation event
        public virtual void TriggerFinalizeDeath()
        {
            FinalizeDeathTrigger?.Invoke();
        }

        public virtual async Task FinalizeDeath(CancellationToken token)
        {
            if (delayBeforeFinalizeDeath)
            {
                await MiscTools.DelayGameTime(timeToDelayBeforeFinalizeDeath, token);
            }
            if (finalizeDeathViaTrigger)
            {
                var tcs = new TaskCompletionSource<object>();
                using var reg = token.Register(Cancel);

                void Cancel()
                {
                    tcs.TrySetCanceled();
                }
                void Complete()
                {
                    tcs.TrySetResult(null);
                }

                try
                {
                    FinalizeDeathTrigger += Complete;
                    await tcs.Task;
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                finally
                {
                    FinalizeDeathTrigger -= Complete;
                }
            }

            if (dropLootOnFinalizeDeath)
            {
                DropLoot();
            }

            DeathFinalized?.Invoke();
            AfterDeathFinalized?.Invoke();

            if (destroyOnFinalizeDeath)
            {
                if (gameObject)
                {
                    Destroy(gameObject);
                }
            }
        }

        public virtual void OnOutOfBounds()
        {
            Instakill();
        }


        //EQUIPMENT

        //if item slot has a SavableMonobehavior equipped, then this will execute after
        //the item slot has restored state, so it will only use the item slot's defaultItemSO (SO!)
        //if no item was saved to that slot
        public void EquipDefaultItems()
        {
            foreach (var entry in equipSlots)
            {
                var slot = entry.Value;
                if (!slot)
                {  
                    continue;
                }
                if (slot.defaultItem == null)
                {
                    slot.InitializeDefaultItemFromSO();

                    if (entry.Key == EquipmentSlot.Mainhand && slot.defaultItem == null)
                    {
                        slot.defaultItem = unarmedWeapon;
                    }
                }
                if (slot.defaultItem != null)
                {
                    if (CanEquip(slot.defaultItem))
                    {
                        EquipItem(slot.defaultItem);
                    }
                    else
                    {
                        HandleUnequippedItem(slot.defaultItem);
                    }
                }
            }
        }

        public virtual void InitializeUnarmedWeapon()
        {
            unarmedWeapon = (Weapon)unarmedWeaponSO.CreateInstanceOfItem();
        }

        public bool CanEquip(EquippableItem item)
        {
            if (item == null || !equipSlots.TryGetValue(item.EquippableItemData.Slot, out var slot) || slot == null)
            {
                return false;
            }

            if (item.EquippableItemData.LevelReqs != null)
            {
                foreach (var req in item.EquippableItemData.LevelReqs)
                {
                    if (progressionManager.GetLevel(req.Skill) < req.Level)
                    {
                        OnEquipmentLevelReqFailed();
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual void OnEquipmentLevelReqFailed() { }

        public void EquipItem(EquippableItem item, bool handleUnequippedItem = true)
        {
            if (!CanEquip(item)) return;

            EquipmentSlot slot = item.EquippableItemData.Slot;

            ItemSlot equipSlot = equipSlots[slot];
            EquippableItem oldItem = equipSlot.EquippedItem;
            if (item != unarmedWeapon)
            {
                equipSlot.EquipItem(item);
            }

            if (item is Weapon weapon && slot == EquipmentSlot.Mainhand)
            {
                equippedWeapon = weapon;
                OnWeaponEquip?.Invoke();
                //if (oldItem == unarmedWeapon)
                //{
                //    return;
                //    //so that we don't handle unequipped in this case
                //}
            }

            if (handleUnequippedItem)
            {
                HandleUnequippedItem(oldItem);
            }
        }

        public void UnequipItem(EquipmentSlot slot, bool handleUnequippedItem = true)
        {
            equipSlots.TryGetValue(slot, out var equipSlot);
            if (equipSlot == null) return;

            var oldItem = equipSlot.EquippedItem;
            var newItem = slot == EquipmentSlot.Mainhand ? unarmedWeapon : null;
            equipSlot.EquipItem(newItem);

            if (slot == EquipmentSlot.Mainhand)
            {
                equippedWeapon = unarmedWeapon;
                OnWeaponEquip?.Invoke();
            }

            if (handleUnequippedItem)
            {
                HandleUnequippedItem(oldItem);
            }
        }

        public virtual void HandleUnequippedItem(EquippableItem item)
        {
            if (item == null || item.Equals(unarmedWeapon)) return;
            inventory.DistributeToFirstAvailableSlots(item.ToInventorySlotData());
        }

        
        //INVENTORY AND LOOT

        public virtual void DropLoot()
        {
            DropLoot(inventory.RemoveAllItems());
        }

        public void DropLoot(IInventorySlotDataContainer loot)
        {
            dropSpawner.SpawnDrop(DropPosition, loot);
        }

        public void DropLoot(IInventorySlotDataContainer[] loot)
        {
            dropSpawner.SpawnDrop(DropPosition, loot);
        }

        public void TakeLoot(IInventorySlotDataContainer loot, bool handleOverflow = true)
        {
            var leftOvers = inventory.DistributeToFirstAvailableSlots(loot);
            if (handleOverflow)
            {
                HandleInventoryOverflow(leftOvers);
            }
        }

        public void TakeLoot(IInventorySlotDataContainer[] loot, bool handleOverflow = true)
        {
            var leftOvers = inventory.DistributeToFirstAvailableSlots(loot);
            if (handleOverflow)
            {
                HandleInventoryOverflow(leftOvers);
            }
        }

        public void ReleaseFromSlot(int i, int quantity = 1)
        {
            dropSpawner.SpawnDrop(transform.position, inventory.RemoveFromSlot(i, quantity));
        }

        public void HandleInventoryOverflow(IInventorySlotDataContainer data)
        {
            if (data?.Item != null && data.Quantity > 0)
            {
                DropLoot(data);
                OnInventoryOverflow?.Invoke();
            }
        }

        public void HandleInventoryOverflow(IInventorySlotDataContainer[] data)
        {
            if (data == null) return;

            //writing it this way has a purpose -- checks if data contains any nontrivial slots,
            //and drops ALL loot as soon as it finds a nontrivial slot
            //(so that all items are sent in one drop)
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i]?.Item != null && data[i].Quantity > 0)
                {
                    DropLoot(data);
                    OnInventoryOverflow?.Invoke();
                    return;
                }
            }
        }


        //TARGETING

        public bool CanAttack(IHealth health)
        {
            if (health == null || health.IsDead || this.health.IsDead)
            {
                return false;
            }
            return CanAttackAtDistSqrd(Vector2.SqrMagnitude(health.transform.position - transform.position),
                health.TargetingTolerance + this.health.TargetingTolerance);
        }

        public bool CanAttackAtDistSqrd(float distanceSqrd, float tolerance)
        {
            return CanAttackAtDistSqrd(distanceSqrd, tolerance, AttackRange);
        }

        public bool CanAttackAtDistSqrd(float distanceSqrd, float tolerance, float attackRange)
        {
            var a = attackRange + tolerance;
            return distanceSqrd < a * a;
        }

        public bool TargetInRange(IHealth target)
        {
            if (!CanAttack(target))
            {
                OnTargetingFailed?.Invoke();
                return false;
            }
            return true;
        }

        public virtual IHealth FindTarget(Vector2 position, float searchRadius)
        {
            var t = Physics2D.OverlapCircleAll(position, searchRadius, targetLayerMask);

            foreach (var c in t)
            {
                var h = GetHealthComponent(c);

                if (h != null && !h.IsDead)
                {
                    return h;
                }
            }

            OnTargetingFailed?.Invoke();
            return null;

            //Collider2D enemyCollider = Physics2D.OverlapCircle(position, searchRadius,
            //    _targetLayer);

            //if (enemyCollider)
            //{
            //    var health = GetHealthComponent(enemyCollider);
            //    if (health == null)
            //    {
            //        OnTargetingFailed?.Invoke();
            //    }
            //    return health;
            //}
            //return null;
        }


        //TRIGGER STATE CHANGES

        public void TriggerCombat()
        {
            Trigger(typeof(InCombat).Name);
        }

        public void Die(IDamageDealer killer = null)
        {
            health.Die(killer);
            Trigger(typeof(Dead).Name);
        }

        public void Instakill()
        {
            HandleHealthChange(Mathf.Infinity, null);
        }

        public void Revive()
        {
            Trigger(typeof(NotInCombat).Name);
            health.Revive();
        }

        public void CombatantReset()
        {
            ReturnQueuedProjectileToPool();
            ReleaseBowPull();
        }


        //FUNCTIONS FOR PROJECTILES AND RANGED WEAPONS

        public void PrepareProjectile(IProjectile projectile, Func<Vector2> getAimPos, float powerMultiplier,
            Func<Collider2D, IHealth> hitAction, int maxHits = 1)
        {
            ReturnQueuedProjectileToPool();
            if (projectile == null)
            {
                Debug.Log($"{gameObject.name} tried to prepare a projectile, but the projectile was null.");
                return;
            }
            projectile.Prepare(this, getAimPos, powerMultiplier, hitAction, maxHits);
            QueuedProjectile = projectile;
        }

        //NOTE: cc should StoreAction(ShootQueuedProjectile) rather than calling this directly
        //(if you want to channel while projectile is queued)
        public void ShootQueuedProjectile()
        {
            QueuedProjectile?.Shoot();
            QueuedProjectile = null;
        }

        //If you want to shoot the projectile immediately (currently not in use)
        public void PrepareAndShootProjectile(IProjectile projectile, Func<Vector2> getAimPos, 
            float powerMultiplier, Func<Collider2D, IHealth> hitAction, int maxHits = 1)
        {
            if (projectile == null)
            {
                Debug.Log($"{gameObject.name} tried to shoot a projectile, but the projectile was null.");
                return;
            }
            projectile.Prepare(this, getAimPos, powerMultiplier, hitAction, maxHits);
            projectile.Shoot();
        }

        public void ReturnQueuedProjectileToPool()
        {
            if(QueuedProjectile != null && QueuedProjectile is IPoolableObject poolable)
            {
                poolable.ReturnToPool();
                QueuedProjectile = null;
            }
        }

        public void PullBow()//these are called by animation events I think?
        {
            if(mainhandSlot.EquippedItemGO && mainhandSlot.EquippedItemGO.TryGetComponent(out Bow bow))
            {
                bow.BeginPull(offhandSlot.transform);
            }
        }

        public void ReleaseBowPull()
        {
            if (mainhandSlot.EquippedItemGO && mainhandSlot.EquippedItemGO.TryGetComponent(out Bow bow))
            {
                bow.ReleasePull();
            }
        }

        protected override void OnDestroy()
        {
            stamina.Depleted -= OnStaminaDepleted;
            wrath.Depleted -= OnWrathDepleted;
            OnTargetingFailed = null;
            OnWeaponEquip = null;
            OnInventoryOverflow = null;
            FinalizeDeathTrigger = null;
            DeathFinalized = null;
            AfterDeathFinalized = null;
            if (progressionManager)
            {
                progressionManager.LevelUp -= OnLevelUp;
                progressionManager.StateRestored -= OnProgressionManagerStateRestored;
            }
        }
    }
}