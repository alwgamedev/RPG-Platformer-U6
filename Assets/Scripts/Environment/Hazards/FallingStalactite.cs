using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    [RequireComponent(typeof(BreakableObject))]
    public class FallingStalactite : PoolableObject, IDamageDealer
    {
        [SerializeField] float impactDamage;

        Rigidbody2D containerRb;
        Collider2D triggerCollider;
        BreakableObject breakableObject;
        State state;

        enum State
        {
            dormant, falling, broken
        }

        public CombatStyle CurrentCombatStyle => CombatStyle.Unarmed;

        private void Awake()
        {
            breakableObject = GetComponent<BreakableObject>();
            containerRb = GetComponent<Rigidbody2D>();
            triggerCollider = GetComponent<Collider2D>();
            state = State.dormant;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Release();
            }
        }


        //BASIC FUNCTIONS

        private void Release()
        {
            if (state != State.dormant) return;

            containerRb.bodyType = RigidbodyType2D.Dynamic;
            state = State.falling;
        }

        private void DamagePlayer(float damage)
        {
            AttackAbility.DealDamage(this, GlobalGameTools.Instance.Player.Combatant.Health, damage);
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy || state != State.falling || breakableObject.HasBroken) return;
            state = State.broken;
            //triggerCollider.enabled = false;
            if (collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                DamagePlayer(impactDamage);
            }
            breakableObject.Break();
            //NOTE: breakable object will disable container collider and destroy game object
        }

        //POOLABLE OBJECT METHODS

        public override void Configure(object parameters) { }

        public override void BeforeSetActive() { }

        public override void AfterSetActive() { }

        public override void ResetPoolableObject() { }
    }
}