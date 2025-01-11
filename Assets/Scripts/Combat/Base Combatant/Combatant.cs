using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatformer.Core;
using RPGPlatformer.UI;
using RPGPlatformer.Effects;
using RPGPlatformer.Inventory;
using RPGPlatformer.Loot;
using RPGPlatformer.Skills;

namespace RPGPlatformer.Combat
{
    using static ItemSlot;

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
        [SerializeField] protected ItemSlot mainhandSlot;
        [SerializeField] protected ItemSlot offhandSlot;
        [SerializeField] protected Transform mainhandElbow;
        [SerializeField] protected Transform chestBone;
        [SerializeField] protected WeaponSO weaponSO;
        [SerializeField] protected ReplenishableStat stamina = new();
        [SerializeField] protected ReplenishableStat wrath = new();

        protected CharacterProgressionManager progressionManager;
        protected InventoryManager inventory;
        protected Dictionary<EquipmentSlots, ItemSlot> equipSlots = new();
        protected DropSpawner dropSpawner;
        protected Weapon weapon;
        protected Health health;

        public string DisplayName => $"<b>{displayName}</b>";
        public int CombatLevel => progressionManager.CombatLevel;
        public bool IsPlayer { get; protected set; }
        public string TargetLayer => targetLayer;
        public string TargetTag => targetTag;
        //public float AdditiveDamageBonus => 0;//EVENTUALLY: will compute based on equipment, stats, and any active buffs
        public InventoryManager Inventory => inventory;
        public Dictionary<EquipmentSlots, ItemSlot> EquipSlots => equipSlots;
        public Transform Transform => transform;
        public Transform MainhandElbow => mainhandElbow;
        public Transform ChestBone => chestBone;
        public IWeapon Weapon => weapon;
        public CombatStyle? CurrentCombatStyle => weapon?.CombatStyle;
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

            IsPlayer = CompareTag("Player");

            equipSlots = new()
            {
                [EquipmentSlots.Head] = headSlot,
                [EquipmentSlots.Torso] = torsoSlot,
                [EquipmentSlots.Mainhand] = mainhandSlot,
                [EquipmentSlots.Offhand] = offhandSlot
            };
        }

        private void OnEnable()
        {
            if (IsPlayer)
            {
                Health.Stat.statBar = GameObject.Find("Player Health Bar").GetComponent<StatBarItem>();
                Stamina.statBar = GameObject.Find("Player Stamina Bar").GetComponent<StatBarItem>();
                Wrath.statBar = GameObject.Find("Player Wrath Bar").GetComponent<StatBarItem>();
            }

            stamina.autoReplenish = true;
        }

        private void Start()
        {
            stamina.Start();
            wrath.Start();
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
            if (weapon == null) return 0;
            return AdditiveDamageBonus(weapon.CombatStyle);
        }

        public float AdditiveDamageBonus(CombatStyle combatStyle)
        {
            if(CharacterSkillBook.GetCombatSkill(combatStyle) == null) return 0;
            return 4.5f * progressionManager.GetLevel(CharacterSkillBook.GetCombatSkill(combatStyle));
            //TO-DO: + bonuses from equipment and active buffs
            //NOTE: at max level 40 this gives +180 damage (then times ability's damage multiplier)
        }

        public float DamageTakenMultiplier()
        {
            float defenseProgress = progressionManager.GetLevel(CharacterSkillBook.Defense)
                / CharacterSkillBook.Defense.XPTable.MaxLevel;
            return 1 - (0.1f * defenseProgress);//hence at max defense you get 10% damage reduction
            //TO-DO: factor in armour, DEFENSE LEVEL, and buffs
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

        public void EquipWeaponSO()
        {
            if(weaponSO)
            {
                EquipItem((EquippableItem)weaponSO.CreateInstanceOfItem(), EquipmentSlots.Mainhand);
            }
        }

        public void EquipItem(EquippableItem item, EquipmentSlots slot, bool handleUnequippedItem = true)
        {
            ItemSlot equipSlot = equipSlots[slot];
            EquippableItem oldItem = equipSlot.EquipppedItem;
            equipSlot.EquipItem(item);

            if (item is Weapon weapon && item.EquippableItemData.Slot == EquipmentSlots.Mainhand)
            {
                this.weapon = weapon;
                OnWeaponEquip?.Invoke();
            }

            if (handleUnequippedItem && oldItem != null)
            {
                HandleUnequippedItem(oldItem);
            }
        }

        public void HandleUnequippedItem(EquippableItem item)
        {
            inventory.DistributeToFirstAvailableSlots(item.ToSlotData());
        }

        
        //INVENTORY AND LOOT

        public void DropLoot()
        {
            DropLoot(inventory.RemoveAllItems());
        }

        public void DropLoot(IInventorySlotDataContainer loot)
        {
            dropSpawner.SpawnDrop(transform.position, loot);
        }

        public void DropLoot(IInventorySlotDataContainer[] loot)
        {
            dropSpawner.SpawnDrop(transform.position, loot);
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
            dropSpawner.SpawnDrop(transform.position, inventory.RemoveFromSlot(i, quantity));
        }

        public void HandleInventoryOverflow(IInventorySlotDataContainer data)
        {
            if (data?.Item() != null && data.Quantity() > 0)
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
                if (data[i]?.Item() != null && data[i].Quantity() > 0)
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
            if (health == null || health.IsDead) return false;
            return CanAttack(Vector3.Distance(health.Transform.position, transform.position));
        }

        public bool CanAttack(float distance)
        {
            return weapon != null && distance < weapon.WeaponStats.AttackRange;
        }

        public void CheckIfTargetInRange(IHealth target, out bool result)
        {
            if (!CanAttack(target))
            {
                result = false;
                OnTargetingFailed?.Invoke();
            }
            else
            {
                result = true;
            }
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
            //ResetBow();
            ReleaseBowPull();
        }


        //FUNCTIONS FOR PROJECTILES AND RANGED WEAPONS

        public void PrepareProjectile(IProjectile projectile, Vector2 aimPos, float powerMultiplier, Action<Collider2D> hitAction, int maxHits = 1)
        {
            ReturnQueuedProjectileToPool();
            if (projectile == null)
            {
                Debug.Log($"{gameObject.name} tried to prepare a projectile, but the projectile was null.");
                return;
            }
            projectile.Prepare(this, aimPos, powerMultiplier, hitAction, maxHits);
            QueuedProjectile = projectile;
        }

        //Triggered by animation events
        public void ShootQueuedProjectile()
        {
            if (QueuedProjectile != null)
            {
                QueuedProjectile.Shoot();
                QueuedProjectile = null;
            }
            else
            {
                Debug.Log($"{gameObject.name} tried to shoot queued projectile, but it was null." +
                    "\n(Either the projectile could not be found or a non-projectile ability is using a projectile animation.)");
            }
        }

        //If you want to shoot the projectile immediately (currently not in use)
        public void PrepareAndShootProjectile(IProjectile projectile, Vector2 aimPos, 
            float powerMultiplier, Action<Collider2D> hitAction, int maxHits = 1)
        {
            if (projectile == null)
            {
                Debug.Log($"{gameObject.name} tried to shoot a projectile, but the projectile was null.");
                return;
            }
            projectile.Prepare(this, aimPos, powerMultiplier, hitAction, maxHits);
            projectile.Shoot(/*forceMultiplier*/);
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