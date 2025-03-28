using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.Effects;
using RPGPlatformer.Inventory;
using RPGPlatformer.Loot;
using RPGPlatformer.Skills;

namespace RPGPlatformer.Combat
{
    [RequireComponent(typeof(CharacterProgressionManager))]
    [RequireComponent(typeof(InventoryManager))]
    [RequireComponent(typeof(DropSpawner))]
    [RequireComponent(typeof(Health))]
    public class Combatant : StateDriver, ICombatant, IInventoryOwner, ILooter, ILootDropper
    {
        [SerializeField] protected string displayName;
        [SerializeField] protected string targetLayer;
        [SerializeField] protected string targetTag;
        [SerializeField] protected ItemSlot headSlot;
        [SerializeField] protected ItemSlot torsoSlot;
        [SerializeField] protected ItemSlot legsSlot;
        [SerializeField] protected ItemSlot mainhandSlot;
        [SerializeField] protected ItemSlot offhandSlot;
        [SerializeField] protected Transform mainhandElbow;
        [SerializeField] protected Transform chestBone;
        [SerializeField] protected WeaponSO defaultWeaponSO;
        [SerializeField] protected WeaponSO unarmedWeaponSO;
        [SerializeField] protected ReplenishableStat stamina = new();
        [SerializeField] protected ReplenishableStat wrath = new();
        [SerializeField] protected bool useAutoCalculatedHealthPoints;

        protected CharacterProgressionManager progressionManager;
        protected InventoryManager inventory;
        protected Dictionary<EquipmentSlot, ItemSlot> equipSlots = new();
        protected DropSpawner dropSpawner;
        protected Weapon equippedWeapon;
        protected Weapon defaultWeapon;
        protected Weapon unarmedWeapon;
        protected Health health;

        public string DisplayName => $"<b>{displayName}</b>";
        public int CombatLevel => progressionManager.CombatLevel;
        public virtual bool IsPlayer => false;
        public string TargetLayer => targetLayer;
        public string TargetTag => targetTag;
        public InventoryManager Inventory => inventory;
        public Dictionary<EquipmentSlot, ItemSlot> EquipSlots => equipSlots;
        //public Transform transform => base.transform;
        public Transform MainhandElbow => mainhandElbow;
        public Transform ChestBone => chestBone;
        public float AttackRange { get; protected set; }
        public IWeapon EquippedWeapon => equippedWeapon;
        public IWeapon DefaultWeapon => defaultWeapon;
        public IWeapon UnarmedWeapon => unarmedWeapon;
        public CombatStyle CurrentCombatStyle => equippedWeapon?.CombatStyle ?? CombatStyle.Unarmed;
        public IProjectile QueuedProjectile { get; set; }
        public IHealth Health => health;
        public ReplenishableStat Stamina => stamina;
        public ReplenishableStat Wrath => wrath;

        public event Action OnTargetingFailed;
        public event Action OnWeaponEquip;
        public event Action OnInventoryOverflow;

        protected virtual void Awake()
        {
            health = GetComponent<Health>();

            progressionManager = GetComponent<CharacterProgressionManager>();
            inventory = GetComponent<InventoryManager>();
            dropSpawner = GetComponent<DropSpawner>();

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

            InitializeWeaponSOs();
        }

        private void Start()
        {
            ConfigureReplenishableStats();
        }

        private void Update()
        {
            //auto-replenish stats if enabled
            stamina.Update();
            wrath.Update();
        }


        //DAMAGE MODIFIERS

        public float AdditiveDamageBonus()
        {
            float total = 0;
            foreach (var entry in equipSlots)
            {
                if (entry.Value != null)
                {
                    total += entry.Value.EquipppedItem?.EquippableItemData.DamageBonus ?? 0;
                }
            }

            if (equippedWeapon != null)
            {
                total += LevelBasedDamageBonus(equippedWeapon.CombatStyle);
            }
            return total;
        }

        public float LevelBasedDamageBonus(CombatStyle combatStyle)
        {
            if(CharacterSkillBook.GetCombatSkill(combatStyle) == null) return 0;
            return 4.5f * progressionManager.GetLevel(CharacterSkillBook.GetCombatSkill(combatStyle));
            //at max level 40 this gives +180 damage (then times ability's damage multiplier)
        }

        public float DamageTakenMultiplier()
        {
            return Mathf.Max(1 - LevelBasedDamageReduction() - (DefenseBonus() / 500), 0);
        }

        public float LevelBasedDamageReduction()
        {
            float defenseProgress = progressionManager.GetLevel(CharacterSkillBook.Defense)
                / CharacterSkillBook.Defense.XPTable.MaxLevel;
            return 0.1f * defenseProgress;
            //hence at max defense you get 10% universal damage reduction
        }

        public float DefenseBonus()
        {
            float total = 0;
            foreach (var entry in equipSlots)
            {
                if (entry.Value != null)
                {
                    total += entry.Value.EquipppedItem?.EquippableItemData.DefenseBonus ?? 0;
                }
            }
            return total;
        }


        //STATS

        protected virtual void ConfigureReplenishableStats()
        {
            if(useAutoCalculatedHealthPoints)
            {
                Health.Stat.SetMaxAndDefaultValue(progressionManager.AutoCalculatedHealthPoints());
            }

            stamina.autoReplenish = true;
            //health and wrath auto-replenish will change when you enter and exit combat.
            //this is set up by the combat manager

            Health.Stat.TakeDefaultValue();
            stamina.TakeDefaultValue();
            wrath.TakeDefaultValue();
        }

        protected virtual void UpdateAttackRange()
        {
            AttackRange = equippedWeapon?.WeaponStats.AttackRange ?? 0;
        }


        //BASIC FUNCTIONS

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
            mainhandSlot.gameObject.SetActive(false);
            DropLoot();
        }


        //EQUIPMENT

        public void EquipDefaultWeapon()
        //caution these should really only be used in start because
        //they add the item to your inventory
        //(which runs the risk of creating duplicates of the item in your inventory)
        {
            if (!CanEquip(defaultWeapon))
            {
                EquipItem(unarmedWeapon);
                HandleUnequippedItem(defaultWeapon);
            }
            else
            {
                EquipItem(defaultWeapon);
            }
        }

        public void EquipDefaultArmour()
        //caution these should really only be used in start because
        //they add the item to your inventory
        //(which runs the risk of creating duplicates of the item in your inventory)
        {
            foreach (var entry in equipSlots)
            {
                if (entry.Value == null 
                    || entry.Key == EquipmentSlot.Mainhand 
                    || entry.Key == EquipmentSlot.Offhand)
                {
                    continue;
                }

                var item = entry.Value.DefaultItem;
                if (!CanEquip(item))
                { 
                    HandleUnequippedItem(item);
                }
                else
                {
                    EquipItem(item);
                }
            }
        }

        public virtual void InitializeWeaponSOs()
        {
            defaultWeapon = CreateWeaponFromSO(defaultWeaponSO);
            unarmedWeapon = CreateWeaponFromSO(unarmedWeaponSO);
        }

        public Weapon CreateWeaponFromSO(WeaponSO weaponSO)
        {
            if(weaponSO)
            {
                return (Weapon)weaponSO.CreateInstanceOfItem();
            }
            return null;
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
            EquippableItem oldItem = equipSlot.EquipppedItem;
            if (item != unarmedWeapon)
            {
                equipSlot.EquipItem(item);
            }

            if (item is Weapon weapon && slot == EquipmentSlot.Mainhand)
            {
                equippedWeapon = weapon;
                OnWeaponEquip?.Invoke();
                if (oldItem == unarmedWeapon)
                {
                    return;
                    //so that we don't handle unequipped in this case
                }
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

            var oldItem = equipSlot.EquipppedItem;
            var newItem = slot == EquipmentSlot.Mainhand ? unarmedWeapon : null;
            equipSlot.EquipItem(newItem);

            if (slot == EquipmentSlot.Mainhand)
            {
                equippedWeapon = unarmedWeapon;
                OnWeaponEquip?.Invoke();
                if (oldItem == unarmedWeapon)
                {
                    return;
                    //so that we don't HandleUnequippedItem and redistribute it to our inventory
                }
            }

            if (handleUnequippedItem)
            {
                HandleUnequippedItem(oldItem);
            }
        }

        public void HandleUnequippedItem(EquippableItem item)
        {
            if (item == null) return;
            inventory.DistributeToFirstAvailableSlots(item.ToSlotData());
        }

        
        //INVENTORY AND LOOT

        public void DropLoot()
        {
            DropLoot(inventory.RemoveAllItems());
        }

        public void DropLoot(IInventorySlotDataContainer loot)
        {
            dropSpawner.SpawnDrop(base.transform.position, loot);
        }

        public void DropLoot(IInventorySlotDataContainer[] loot)
        {
            dropSpawner.SpawnDrop(base.transform.position, loot);
        }

        public void TakeLoot(IInventorySlotDataContainer loot)
        {
            HandleInventoryOverflow(inventory.DistributeToFirstAvailableSlots(loot));
        }

        public void TakeLoot(IInventorySlotDataContainer[] loot)
        {
            HandleInventoryOverflow(inventory.DistributeToFirstAvailableSlots(loot));
        }

        public void ReleaseFromSlot(int i, int quantity = 1)
        {
            dropSpawner.SpawnDrop(base.transform.position, inventory.RemoveFromSlot(i, quantity));
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
            if (health == null || health.IsDead)
            {
                return false;
            }
            return CanAttack(Vector2.SqrMagnitude(health.transform.position - base.transform.position),
                health.TargetingTolerance + this.health.TargetingTolerance);
        }

        public bool CanAttack(float distanceSqrd, float tolerance)
        {
            var a = AttackRange + tolerance;
            return equippedWeapon != null && distanceSqrd < a * a;
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
            Collider2D enemyCollider = Physics2D.OverlapCircle(position, searchRadius, LayerMask.GetMask(targetLayer));

            if (enemyCollider)
            {
                if (enemyCollider.TryGetComponent(out IHealth health))
                {
                    return health;
                }
            }
            return null;
        }


        //TRIGGER STATE CHANGES

        public void Attack()
        {
            Trigger(typeof(InCombat).Name);
        }

        public void Die(IDamageDealer killer = null)
        {
            health.Die(killer);
            Trigger(typeof(Dead).Name);
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
            Action<Collider2D> hitAction, int maxHits = 1)
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

        //Triggered by animation events
        public void ShootQueuedProjectile()
        {
            QueuedProjectile?.Shoot();
            QueuedProjectile = null;
        }

        //If you want to shoot the projectile immediately (currently not in use)
        public void PrepareAndShootProjectile(IProjectile projectile, Func<Vector2> getAimPos, 
            float powerMultiplier, Action<Collider2D> hitAction, int maxHits = 1)
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
            OnTargetingFailed = null;
            OnWeaponEquip = null;
            OnInventoryOverflow = null;
        }
    }
}