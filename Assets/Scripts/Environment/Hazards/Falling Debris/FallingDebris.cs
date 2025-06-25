using RPGPlatformer.Combat;
using RPGPlatformer.Core;
using UnityEngine;

namespace RPGPlatformer.Environment
{
    public class FallingDebris : PoolableObject, IDamageDealer
    {
        [SerializeField] RandomizableVector3 scale;
        [SerializeField] RandomizableFloat impactDamage;
        [SerializeField] RandomizableColor color;

        SpriteRenderer spriteRenderer;
        Collider2D ceiling;
        ParticleSystem particles;

        public SpriteRenderer SpriteRenderer => spriteRenderer;
        public CombatStyle CurrentCombatStyle => CombatStyle.Unarmed;

        private void Awake()
        {
            //for simplicity, just set the size and color once rather than on every respawn
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.color = color.Value;
            particles = GetComponentInChildren<ParticleSystem>();
            transform.localScale = scale.Value;
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!gameObject.activeInHierarchy || collider == ceiling) return;
            if (collider.transform == GlobalGameTools.Instance.PlayerTransform)
            {
                DamagePlayer(impactDamage.Value);
            }
            ReturnToPool();

        }

        private void DamagePlayer(float damage)
        {
            AttackAbility.DealDamage(this, GlobalGameTools.Instance.Player.Combatant.Health, damage);
        }

        //Poolable Object overrides

        public override void AfterSetActive()
        {
            if (particles)
            {
                particles.Play();
            }
        }

        public override void BeforeSetActive() { }

        public override void Configure(object parameters)
        {
            ceiling = ((GameObject) parameters).GetComponent<Collider2D>();
        }

        public override void ResetPoolableObject()
        {
            if (particles)
            {
                particles.Stop();
            }
        }
    }
}